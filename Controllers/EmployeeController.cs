
using Microsoft.AspNetCore.Mvc; //[Microsoft.AspNetCore.Mvc.HttpGet]
using Microsoft.Extensions.Caching.Memory;////Installing "Microsoft.Extensions.Caching.Memory" from Nuget
using NetCoreWebAPI_InMemoryCash.Models;
using NetCoreWebAPI_InMemoryCash.Services;
using System.Net;


namespace NetCore_InMemoryCash.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private const string employeeListCacheKey = "employeeList";
        private readonly IEmployeeService _dataRepository;

        private readonly IMemoryCache _cache;

        private readonly ILogger<EmployeeController> _logger;
        public EmployeeController(IEmployeeService dataRepository,
            IMemoryCache cache,
            ILogger<EmployeeController> logger)
        {
            _dataRepository = dataRepository ?? throw new ArgumentNullException(nameof(dataRepository));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }

        //Declaring a SemaphoreSlim object in the controller to implement locking of cache:
        private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1); //This will help us control the number of threads that can access a resource concurrently


        [HttpGet("GetEmployee")]
        public async Task<IActionResult> GetEmployee()
        {
            _logger.Log(LogLevel.Information, "Trying to fetch the list of employees from cache.");

            if (_cache.TryGetValue(employeeListCacheKey, out IEnumerable<Employee> employees)) //If the value is available in the cache, then getting the value from the cache and send response to the user
            {
                _logger.Log(LogLevel.Information, "Employee list found in cache.");
            }
            else
            {
                _logger.Log(LogLevel.Information, "Employee list not found in cache. Fetching from database.");

                employees = _dataRepository.GetAll();

                //In-Memory Cache Parameters:
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromSeconds(60)) //How long the cache will be inactive. It means, the cache entry will expire if it is not used by anyone for this particular time period.
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(5)) //Once the time is reached, then the cache entry will be removed.The cache will be expired once the time is reached 5 minutes.Absolute expiration should not less than the Sliding Expiration.
                        .SetPriority(CacheItemPriority.Normal)
                        .SetSize(1024);// Setting a Size Limit, it represents the number of cache entries that the cache can hold. if no cache size limit is set, the size set on individual cache entries will be ignored.

                #region
                /*
                 About limit size:
                 We can create cache entries in different sizes, but once the sum of all entries reaches the SizeLimit, it cannot insert any more entries. 
                For instance, in this example, we could create 1024 entries with size 1, 512 with size 2, or 256 with size 4, etc. 
                The idea is that we can design different cache entries by giving varying sizes depending on the application’s requirement.

                var options = new MemoryCacheEntryOptions().SetSize(2);
                    cache.Set("myKey1", "123", options);
                    cache.Set("myKey2", "456", options);


                An interesting thing to note here is that once the cache reaches its limit, it does not remove the oldest entry to make room for new entries. 
                Instead, it will just ignore the new entries and the cache insert operation will not throw an error as well. 
                So we should take care while designing cache with a size limit or else it won’t be easy to troubleshoot cache-related issues later.

                */
                #endregion

                _cache.Set(employeeListCacheKey, employees, cacheEntryOptions);
            }

            return Ok(employees);
        }
        //----------------------
        //Cache Implementation using lock:

        /// <summary>
        /// Now let’s assume that multiple users try to access the data from In-Memory Cache at the same time. Even though the IMemoryCache is thread-safe, 
        /// it is prone to race conditions. For instance, if the cache is empty and two users try to access data at the same time, 
        /// there is a chance that both users may fetch the data from the database and populate the cache. This is not desirable. 
        /// To solve these kinds of issues, we need to implement a locking mechanism for the cache.
        /// For implementing locking for cache, we can use the SemaphoreSlim class, which is a lightweight version of the Semaphore class. 
        /// This will help us control the number of threads that can access a resource concurrently.
        /// </summary>


        [HttpGet("GetEmployeeAsync")]    
        //[NonAction] //Ignoring by Swagger
        public async Task<IActionResult> GetEmployeeAsync()
        {
            _logger.Log(LogLevel.Information, "Trying to fetch the list of employees from cache.");

            //First, we are checking if the cache has value or not.
            if (_cache.TryGetValue("employeeList", out IEnumerable<Employee> employees))//If the value is available in the cache, then getting the value from the cache and send response to the user
            {
                _logger.Log(LogLevel.Information, "Employee list found in cache.");
            }
            else
            {
                try
                {
                    await semaphore.WaitAsync();//Once a thread has been granted access to the Semaphore, 
                    if (_cache.TryGetValue("employeeList", out employees)) //Recheck if the value has been populated previously for safety (Avoid concurrent thread access).
                    {
                        _logger.Log(LogLevel.Information, "Employee list found in cache.");
                    }
                    else
                    {
                        _logger.Log(LogLevel.Information, "Employee list not found in cache. Fetching from database.");
                        employees = _dataRepository.GetAll();
                        var cacheEntryOptions = new MemoryCacheEntryOptions()
                                .SetSlidingExpiration(TimeSpan.FromSeconds(60))
                                .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600))
                                .SetPriority(CacheItemPriority.Normal)
                                .SetSize(1024);

                        _cache.Set(employeeListCacheKey, employees, cacheEntryOptions);
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            }
            return Ok(employees);
        }
        //--------------------------

        [HttpPost]
        public IActionResult Post([FromBody] Employee employee)
        {
            if (employee == null)
            {
                return BadRequest("Employee is null.");
            }
            _dataRepository.Add(employee);
            _cache.Remove(employeeListCacheKey); //Removing Data From In-Memory Cache
            if (!_cache.TryGetValue("employeeList", out IEnumerable<Employee> employees)) //Recheck if the value has been populated previously for safety (Avoid concurrent thread access).
            {
                _logger.Log(LogLevel.Information, "Cash Removed Successfully!");
                return new ObjectResult(employee) { StatusCode = (int)HttpStatusCode.Created };
            }

            return BadRequest("Removing Cash Faild!");



        }
    }
}
/*

When the application server is running short of memory, 
the .NET Core runtime will initiate the clean-up of In-Memory cache items other than the ones set with NeverRemove priority.
Once we set the sliding expiration, the inactive entries will expire at that time. 
Similarly, once we set the absolute expiration, all entries will expire by that time.
 
 */