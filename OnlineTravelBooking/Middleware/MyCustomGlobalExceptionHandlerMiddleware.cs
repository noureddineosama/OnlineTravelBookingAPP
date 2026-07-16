namespace OnlineTravelBooking.Middleware
{
    public class MyCustomGlobalExceptionHandlerMiddleware 
    {
        private readonly RequestDelegate next;

        public MyCustomGlobalExceptionHandlerMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context); //. making the context or the request arrive
            }
            catch (Exception ex) //. parent Exception
            {
                //. 500 status code if there is a fault in the request
                context.Response.StatusCode = 500;

                //. appearing the message as json 
                await context.Response.WriteAsJsonAsync(new
                {
                    message = ex.Message
                });
            }
        }
    }
}
