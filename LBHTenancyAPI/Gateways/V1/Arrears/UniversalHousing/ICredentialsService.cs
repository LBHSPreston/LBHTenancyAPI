using AgreementService;

namespace LBHTenancyAPI.Gateways.V1.Arrears.UniversalHousing
{
    public interface ICredentialsService
    {
        string GetUhSourceSystem();
        UserCredential GetUhUserCredentials();
    }
}
