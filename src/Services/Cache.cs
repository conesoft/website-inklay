namespace Conesoft.Website.Inklay.Services
{
    public abstract class Cache<T> where T : class
    {
        private readonly HttpClient client;
        private readonly PeriodicTimer timer;

        protected HttpClient Client => client;

        private T? content = null;

        public Cache(IHttpClientFactory factory, TimeSpan period)
        {
            client = factory.CreateClient();
            timer = new(period);

            var _ = StartPeriodicCacheUpdate();
        }

        public async Task<T> GetContent()
        {
            while(content == null)
            {
                await Task.Delay(25);
            }
            return content;
        }

        protected abstract Task<T> Generate();

        public async Task StartPeriodicCacheUpdate()
        {
            do
            {
                content = await Generate();
            }
            while (await timer.WaitForNextTickAsync());
        }
    }
}
