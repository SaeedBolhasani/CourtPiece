using Orleans.Concurrency;
using static AuthService;

namespace CourtPiece.WebApi.Grains
{
    public interface IAuthenticationGrain : IGrainWithIntegerKey
    {
        Task<(int, string)> Registration(Immutable<RegistrationModel> model);
        Task<(int, string)> Login(Immutable<LoginModel> model);
    }

    [StatelessWorker(1000)]
    public class AuthenticationGrain : Grain, IAuthenticationGrain
    {
        private IAuthService authService;

        public async Task<(int, string)> Registration(Immutable<RegistrationModel> model)
        {
            return await this.authService.Registeration(model.Value);
        }

        public async Task<(int, string)> Login(Immutable<LoginModel> model)
        {
            return await authService.Login(model.Value);
        }

        public override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("new grain");
            this.authService = ServiceProvider.GetRequiredService<IAuthService>();
            return base.OnActivateAsync(cancellationToken);
        }

        public override Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
        {
            //this.authService
            Console.WriteLine("grain off");
            return base.OnDeactivateAsync(reason, cancellationToken);
        }
    }
}
