using System;
using System.Collections.Generic;
using System.Security.Principal;

namespace Microsoft.IdentityModel.Claims
{
    /// <summary>
    /// Class that contains <see cref="IClaimsIdentity"/> and <see cref="IPrincipal"/> extension methods.
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        #region action methods
        /// <summary>
        /// Retrieves all of the claims that are matched by the specified predicate.
        /// </summary>
        /// <param name="principal"><see cref="IPrincipal"/> principal object.</param>
        /// <param name="match">The function that performs the matching logic.</param>
        /// <returns>The matching claims.</returns>
        /// <exception cref="ArgumentNullException">Throw when <paramref name="principal"/> or <paramref name="match"/> is <c>null</c> reference.</exception>
        public static IEnumerable<Claim> FindAll(this IPrincipal principal, Predicate<Claim> match)
        {
            if (principal == null)
            {
                throw new ArgumentNullException("principal");
            }
            if (match == null)
            {
                throw new ArgumentNullException("match");
            }

            var claimsPrincipal = ClaimsPrincipal.CreateFromPrincipal(principal);

            var list = new List<Claim>();
            foreach (var identity in claimsPrincipal.Identities)
            {
                foreach (Claim claim in identity.FindAll(match))
                {
                    list.Add(claim);
                }
            }
            return list.AsReadOnly();
        }

        /// <summary>
        /// Retrieves all or the claims that have the specified claim type.
        /// </summary>
        /// <param name="principal"><see cref="IPrincipal"/> principal object.</param>
        /// <param name="type">The claim type against which to match claims.</param>
        /// <returns>The matching claims.</returns>
        /// <exception cref="ArgumentNullException">Throw when <paramref name="principal"/> or <paramref name="type"/> is <c>null</c> reference.</exception>
        public static IEnumerable<Claim> FindAll(this IPrincipal principal, string type)
        {
            if (principal == null)
            {
                throw new ArgumentNullException("principal");
            }
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            var claimsPrincipal = ClaimsPrincipal.CreateFromPrincipal(principal);

            var list = new List<Claim>();
            foreach (var identity in claimsPrincipal.Identities)
            {
                foreach (Claim claim in identity.FindAll(type))
                {
                    list.Add(claim);
                }
            }
            return list.AsReadOnly();
        }

        /// <summary>
        /// Retrieves the first claim that is matched by the specified predicate.
        /// </summary>
        /// <param name="principal"><see cref="IPrincipal"/> principal object.</param>
        /// <param name="match">The function that performs the matching logic.</param>
        /// <returns>The first matching claim or <c>null</c> if no match is found.</returns>
        /// <exception cref="ArgumentNullException">Throw when <paramref name="principal"/> or <paramref name="match"/> is <c>null</c> reference.</exception>
        public static Claim FindFirst(this IPrincipal principal, Predicate<Claim> match)
        {
            if (principal == null)
            {
                throw new ArgumentNullException("principal");
            }
            if (match == null)
            {
                throw new ArgumentNullException("match");
            }

            var claimsPrincipal = ClaimsPrincipal.CreateFromPrincipal(principal);

            foreach (var identity in claimsPrincipal.Identities)
            {
                var claim = identity.FindFirst(match);
                if (claim != null)
                {
                    return claim;
                }
            }
            return null;
        }

        /// <summary>
        /// Retrieves the first claim with the specified claim type.
        /// </summary>
        /// <param name="principal"><see cref="IPrincipal"/> principal object.</param>
        /// <param name="type">The claim type to match.</param>
        /// <returns>The first matching claim or <c>null</c> if no match is found.</returns>
        /// <exception cref="ArgumentNullException">Throw when <paramref name="principal"/> or <paramref name="type"/> is <c>null</c> reference.</exception>
        public static Claim FindFirst(this IPrincipal principal, string type)
        {
            if (principal == null)
            {
                throw new ArgumentNullException("principal");
            }
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            var claimsPrincipal = ClaimsPrincipal.CreateFromPrincipal(principal);

            foreach (var identity in claimsPrincipal.Identities)
            {
                var claim = identity.FindFirst(type);
                if (claim != null)
                {
                    return claim;
                }
            }
            return null;
        }

        /// <summary>
        /// Determines whether any of the claims identities associated with this claims principal contains a claim that is matched by the specified predicate.
        /// </summary>
        /// <param name="principal"><see cref="IPrincipal"/> principal object.</param>
        /// <param name="match">The function that performs the matching logic.</param>
        /// <returns><c>true</c> if a matching claim exists; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Throw when <paramref name="principal"/> or <paramref name="match"/> is <c>null</c> reference.</exception>
        public static bool HasClaim(this IPrincipal principal, Predicate<Claim> match)
        {
            if (principal == null)
            {
                throw new ArgumentNullException("principal");
            }
            if (match == null)
            {
                throw new ArgumentNullException("match");
            }

            var claimsPrincipal = ClaimsPrincipal.CreateFromPrincipal(principal);

            foreach (var identity in claimsPrincipal.Identities)
            {
                if (identity.HasClaim(match))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Determines whether any of the claims identities associated with this claims principal contains a claim with the specified claim type and value.
        /// </summary>
        /// <param name="principal"><see cref="IPrincipal"/> principal object.</param>
        /// <param name="type">The type of the claim to match.</param>
        /// <param name="value">The value of the claim to match.</param>
        /// <returns><c>true</c> if a matching claim exists; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Throw when <paramref name="principal"/>, <paramref name="type"/> or <paramref name="value"/> is <c>null</c> reference.</exception>
        public static bool HasClaim(this IPrincipal principal, string type, string value)
        {
            if (principal == null)
            {
                throw new ArgumentNullException("principal");
            }
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            var claimsPrincipal = ClaimsPrincipal.CreateFromPrincipal(principal);

            foreach (var identity in claimsPrincipal.Identities)
            {
                if (identity.HasClaim(type, value))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns a collection that contains all of the claims from all of the claims identities associated with this claims principal.
        /// </summary>
        /// <param name="principal"><see cref="IPrincipal"/> principal object.</param>
        /// <returns>The claims associated with this principal.</returns>
        /// <exception cref="ArgumentNullException">Throw when <paramref name="principal"/> is <c>null</c> reference.</exception>
        public static IEnumerable<Claim> Claims(this IPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException("principal");
            }

            var claimsPrincipal = ClaimsPrincipal.CreateFromPrincipal(principal);

            foreach (var identity in claimsPrincipal.Identities)
            {
                if (identity.Claims != null)
                {
                    foreach (Claim claim in identity.Claims)
                    {
                        yield return claim;
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves all of the claims that are matched by the specified predicate.
        /// </summary>
        /// <param name="identity"><see cref="IClaimsIdentity"/> identity object.</param>
        /// <param name="match">The function that performs the matching logic.</param>
        /// <returns>The matching claims. The list is read-only.</returns>
        /// <exception cref="ArgumentNullException">Throw when <paramref name="identity"/> or <paramref name="match"/> is <c>null</c> reference.</exception>
        public static IEnumerable<Claim> FindAll(this IClaimsIdentity identity, Predicate<Claim> match)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }
            if (match == null)
            {
                throw new ArgumentNullException("match");
            }

            var list = new List<Claim>();
            if (identity.Claims != null)
            {
                foreach (Claim claim in identity.Claims)
                {
                    if (match(claim))
                    {
                        list.Add(claim);
                    }
                }
            }
            return list.AsReadOnly();
        }

        /// <summary>
        /// Retrieves all of the claims that have the specified claim type.
        /// </summary>
        /// <param name="identity"><see cref="IClaimsIdentity"/> identity object.</param>
        /// <param name="type">The claim type against which to match claims.</param>
        /// <returns>The matching claims. The list is read-only.</returns>
        /// <exception cref="ArgumentNullException">Throw when <paramref name="identity"/> or <paramref name="type"/> is <c>null</c> reference.</exception>
        public static IEnumerable<Claim> FindAll(this IClaimsIdentity identity, string type)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            var list = new List<Claim>();
            if (identity.Claims != null)
            {
                foreach (Claim claim in identity.Claims)
                {
                    if (string.Equals(claim.ClaimType, type, StringComparison.OrdinalIgnoreCase))
                    {
                        list.Add(claim);
                    }
                }
            }
            return list.AsReadOnly();
        }

        /// <summary>
        /// Retrieves the first claim that is matched by the specified predicate.
        /// </summary>
        /// <param name="identity"><see cref="IClaimsIdentity"/> identity object.</param>
        /// <param name="match">The function that performs the matching logic.</param>
        /// <returns>The first matching claim or <c>null</c> if no match is found.</returns>
        /// <exception cref="ArgumentNullException">Throw when <paramref name="identity"/> or <paramref name="match"/> is <c>null</c> reference.</exception>
        public static Claim FindFirst(this IClaimsIdentity identity, Predicate<Claim> match)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }
            if (match == null)
            {
                throw new ArgumentNullException("match");
            }

            if (identity.Claims == null)
            {
                return null;
            }

            foreach (Claim claim in identity.Claims)
            {
                if (match(claim))
                {
                    return claim;
                }
            }
            return null;
        }

        /// <summary>
        /// Retrieves the first claim with the specified claim type.
        /// </summary>
        /// <param name="identity"><see cref="IClaimsIdentity"/> identity object.</param>
        /// <param name="type">The claim type to match.</param>
        /// <returns>The first matching claim or <c>null</c> if no match is found.</returns>
        /// <exception cref="ArgumentNullException">Throw when <paramref name="identity"/> or <paramref name="type"/> is <c>null</c> reference.</exception>
        public static Claim FindFirst(this IClaimsIdentity identity, string type)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (identity.Claims == null)
            {
                return null;
            }

            foreach (Claim claim in identity.Claims)
            {
                if (string.Equals(claim.ClaimType, type, StringComparison.OrdinalIgnoreCase))
                {
                    return claim;
                }
            }
            return null;
        }

        /// <summary>
        /// Determines whether this claims identity has a claim that is matched by the specified predicate.
        /// </summary>
        /// <param name="identity"><see cref="IClaimsIdentity"/> identity object.</param>
        /// <param name="match">The function that performs the matching logic.</param>
        /// <returns><c>true</c> if a matching claim exists; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Throw when <paramref name="identity"/> or <paramref name="match"/> is <c>null</c> reference.</exception>
        public static bool HasClaim(this IClaimsIdentity identity, Predicate<Claim> match)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }
            if (match == null)
            {
                throw new ArgumentNullException("match");
            }

            if (identity.Claims == null)
            {
                return false;
            }

            foreach (Claim claim in identity.Claims)
            {
                if (match(claim))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Determines whether this claims identity has a claim with the specified claim type and value.
        /// </summary>
        /// <param name="identity"><see cref="IClaimsIdentity"/> identity object.</param>
        /// <param name="type">The type of the claim to match.</param>
        /// <param name="value">The value of the claim to match.</param>
        /// <returns><c>true</c> if a match is found; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Throw when <paramref name="identity"/>, <paramref name="type"/> or <paramref name="value"/> is <c>null</c> reference.</exception>
        public static bool HasClaim(this IClaimsIdentity identity, string type, string value)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            if (identity.Claims == null)
            {
                return false;
            }

            foreach (Claim claim in identity.Claims)
            {
                if (string.Equals(claim.ClaimType, type, StringComparison.OrdinalIgnoreCase) && string.Equals(claim.Value, value, StringComparison.Ordinal))
                {
                    return true;
                }
            }
            return false;
        }
        #endregion
    }
}