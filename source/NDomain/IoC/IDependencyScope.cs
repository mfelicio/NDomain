namespace NDomain.IoC
{
    /// <summary>
    /// IDependencyScope abstraction used when resolving message handlers, so that each message is processed within its own dependency scope.
    /// </summary>
    public interface IDependencyScope : IDependencyResolver
    {
        
    }
}