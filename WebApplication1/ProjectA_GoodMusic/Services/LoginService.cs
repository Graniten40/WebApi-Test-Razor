using Models.DTO;

namespace Services;

public class LoginService : ILoginService
{
    public Task<ResponseItemDto<LoginUserSessionDto>> LoginUserAsync(LoginCredentialsDto usrCreds)
    {
        // TODO: riktig login-logik senare.
        throw new NotImplementedException();
    }
}
