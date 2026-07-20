using System.Diagnostics;

namespace OnlineTravelBooking.Middleware
{
    public class MeasuringExecutingTimeMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<MeasuringExecutingTimeMiddleware> logger;

        public MeasuringExecutingTimeMiddleware(RequestDelegate next, ILogger<MeasuringExecutingTimeMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var timer = Stopwatch.StartNew(); 
            await next(context);
            timer.Stop();

            var ExecutionTime = timer.ElapsedMilliseconds;
            logger.LogInformation($"Request Path : {context.Request.Path} " +
                $"took {ExecutionTime} ms");
        }
    }
}
