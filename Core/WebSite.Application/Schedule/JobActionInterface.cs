namespace Website.Application.Schedule
{
    public interface JobActionInterface
    {
        void Run(JobBase job);
    }
}