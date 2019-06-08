using System;
using System.IdentityModel.Services;

namespace IMP.Shared
{
    public class SafeSessionAuthenticationModule : SessionAuthenticationModule
    {
        #region private member functions
        protected override void OnAuthenticateRequest(object sender, EventArgs eventArgs)
        {
            try
            {
                base.OnAuthenticateRequest(sender, eventArgs);
            }
            catch (InvalidOperationException ex)
            {
                if (ex.InnerException is System.Security.Cryptography.CryptographicException && ex.Message.StartsWith("ID1073: "))  //Key not valid for use in specified state.
                {
                    //Změna app pool identity, zapamatované přihlášení je neplatné
                    this.SignOut();
                    return;
                }

                throw;
            }
            catch (FederatedAuthenticationSessionEndingException)
            {
                this.SignOut();
            }
            catch (System.IdentityModel.Tokens.SecurityTokenException)
            {
                this.SignOut();
            }
        }
        #endregion
    }
}