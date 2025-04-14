using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using asset_allocation_api.Config;
using asset_allocation_api.Context;
using asset_allocation_api.Model;
using asset_allocation_api.Model.Input_Model;
using asset_allocation_api.Model.Token;
using asset_allocation_api.Models;
using asset_allocation_api.Util;
using System.DirectoryServices.Protocols;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text;
using System.Security.Claims;
using Microsoft.Extensions.Logging.Abstractions;
using Novell.Directory.Ldap;
using Personnel = asset_allocation_api.Context.Personnel;

namespace asset_allocation_api.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController(AppDbContext context, ILogger<LoginController> logger) : ControllerBase
    {
        private readonly AppDbContext _context = context;
        private readonly ILogger<LoginController> _logger = logger;
        private static string _ldapPath = "mnoytcorpdc7.corp.riotinto.org";
        private static int _ldapPort = 3268;

        // Post: api/Login
        [HttpPost]
        public ActionResult<Response<object?>> Login([FromBody] LoginInput loginInput)
        {
            Response<object?> resp = new();
            try
            {
                if (loginInput.Password.IsNullOrEmpty())
                {
                    return ResponseUtils.ReturnResponse(_logger, null, resp, null, 401, false, "The supplied credential is invalid");
                }

                // if (!"-prod".Equals(AssetAllocationConfig.env) && !"passw0rd".Equals(loginInput.Password))
                // {
                //     System.DirectoryServices.Protocols.LdapConnection ldapConnection = new(new LdapDirectoryIdentifier(AssetAllocationConfig.ldapPath), new NetworkCredential(loginInput.Username, loginInput.Password, AssetAllocationConfig.ldapDomainName), AuthType.Basic);
                //     ldapConnection.SessionOptions.ProtocolVersion = 3;
                //     ldapConnection.Bind();
                // }
                _logger.LogInformation("{username} login successful", loginInput.Username);
                //TODO GET PERSONNEL INFO WITH USERNAME
                string email = loginInput.Username;
                // var groups = GetUserGroups(logger, loginInput.Username, loginInput.Password);
                var admingroups = _context.Configurations.Where(a => a.Category == "Role").ToList();

                List<string> localgroups = new List<string>();
                List<string> departments = new List<string>();
                
;                // check admin group and add to claims role 
                Personnel? personnel = _context.Personnel.Where(a => a.Email == email).FirstOrDefault();

                if (personnel == null)
                {
                    return ResponseUtils.ReturnResponse(_logger, null, resp, null, 401, false, $"User not found the system. Email: {email}");
                }
                
                var personnelNoString = personnel.PersonnelNo.ToString();
                var roleConfig = _context.Configurations.Where(p => p.ConfigValue == personnelNoString).ToList();
                foreach (var role in roleConfig)
                {
                    departments.Add(role.DepartmentId.ToString());
                    localgroups.Add(role.ConfigDesc);
                }
                var ci = new ClaimsIdentity();
                //ci.AddClaim(new Claim("id", user.Id.ToString()));
                ci.AddClaim(new Claim(ClaimTypes.Name, loginInput.Username));
                ci.AddClaim(new Claim(ClaimTypes.NameIdentifier, personnel.PersonnelNo.ToString()));
                ci.AddClaim(new Claim(ClaimTypes.GivenName, personnel.FirstName));
                ci.AddClaim(new Claim(ClaimTypes.Email, email));

                foreach (var role in localgroups)
                    ci.AddClaim(new Claim(ClaimTypes.Role, role));

                var deps = String.Join(",", departments.ToArray()); 

                Dictionary<string, object> tokenData = new()
                {
                    { "PersonnelNo",personnel.PersonnelNo},
                    { "PersonnelId",personnel.PersonnelId!},
                    { "FirstName",personnel.FirstName !},
                    { "LastName",personnel.LastName !},
                    { "Email",personnel.Email !},
                    { "departments", deps }
                };

                SecurityTokenDescriptor tokenDescriptor = new()
                {
                    Claims = tokenData,
                    Expires = DateTime.UtcNow.AddDays(int.Parse(Config.AssetAllocationConfig.jwtExpiryDays)),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(AssetAllocationConfig.JWT_SECRET)), SecurityAlgorithms.HmacSha256Signature),
                    Subject = ci
                };
                JwtSecurityTokenHandler tokenHandler = new();
                SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

                return ResponseUtils.ReturnResponse(_logger, null, resp, new Token(tokenHandler.WriteToken(token), "", personnel), 200, true, "Successful");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occurred during the LDAP request:{0}", ex.Message);
                if (ex.Message.Contains("The supplied credential is invalid"))
                    return ResponseUtils.ReturnResponse(_logger, ex, resp, null, 401, false, "The supplied credential is invalid");
                else
                    return ResponseUtils.ReturnResponse(_logger, ex, resp, null, 500, false, "Internal error occured");
            }
        }


        // public static List<string> GetUserGroups(
        //   ILogger logger,
        //   string username,
        //   string password)
        // {
        //     username = username.ToLower();
        //     logger ??= NullLogger<LdapUtilities>.Instance;
        //
        //     if (!RegexUtils.regexUserAccount.IsMatch(username))
        //     {
        //         throw new InvalidDataException("Username invalid. Contains illegal character");
        //     }
        //
        //     var resultDict = LdapSearch(
        //         _ldapPath,
        //         _ldapPort,
        //         "CORP\\" + username,
        //         password,
        //         "OU=APAC,OU=PROD,DC=corp,DC=riotinto,DC=org",
        //         "sAMAccountName=" + username,
        //         new[] { "sAMAccountName", "memberOf" },
        //         logger);
        //
        //     return resultDict[username]["memberOf"].Select(str => str.Substring(3, str.IndexOf("OU=", StringComparison.Ordinal) - 4)).ToList();
        // }


        // public static Dictionary<string, Dictionary<string, string[]>> LdapSearch(string ldapPath,int ldapPort,string saUser,string saPass,string searchContainer,string searchFilter,string[] attributes,ILogger? logger = null)
        // {
        //     logger ??= NullLogger<LdapUtilities>.Instance;
        //
        //     using Novell.Directory.Ldap.LdapConnection lc = new();
        //
        //     Novell.Directory.Ldap.LdapSearchConstraints constraint = lc.SearchConstraints;
        //     constraint.ReferralFollowing = true;
        //     lc.Constraints = constraint;
        //
        //     lc.Connect(ldapPath, ldapPort);
        //
        //     lc.Bind(Novell.Directory.Ldap.LdapConnection.LdapV3, saUser, saPass);
        //     //ldapConnection.
        //     var searchResults =
        //         lc.Search(searchContainer,
        //         Novell.Directory.Ldap.LdapConnection.ScopeSub,
        //         searchFilter,
        //         attributes,
        //         false);
        //
        //     Dictionary<string, Dictionary<string, string[]>> resp = new();
        //
        //     logger.LogDebug("searchResults.HasMore(): {searchResults.HasMore()}", searchResults.HasMore());
        //
        //     while (searchResults.HasMore())
        //     {
        //         Dictionary<string, string[]> row = new();
        //         LdapEntry nextEntry;
        //         try
        //         {
        //             nextEntry = searchResults.Next();
        //
        //             foreach (var attr in attributes)
        //             {
        //                 row.Add(attr, nextEntry.GetAttribute(attr).StringValueArray);
        //             }
        //         }
        //         catch (Novell.Directory.Ldap.LdapException e)
        //         {
        //             logger.LogError(e, "LdapException Error");
        //             continue;
        //         }
        //         resp.Add(nextEntry.GetAttribute("sAMAccountName").StringValue.ToLower(), row);
        //         logger.LogDebug("nextEntry.Dn: {nextEntry.Dn}", nextEntry.Dn);
        //     }
        //     return resp;
        // }
    }
}