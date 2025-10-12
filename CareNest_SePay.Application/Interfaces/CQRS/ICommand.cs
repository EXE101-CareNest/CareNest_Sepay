namespace CareNest_SePay.Application.Interfaces.CQRS
{
    public interface ICommand { }
    
    public interface ICommand<TResult> : ICommand { }
}
