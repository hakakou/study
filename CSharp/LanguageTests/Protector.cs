using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;

namespace Haka.Lib
{
    public class User
    {
        public string Name { get; set; }
        public string Salt { get; set; }
        public string SaltedHashedPassword { get; set; }
        public string[] Roles { get; set; }
    }

    public static class Protector
    {
        private static Dictionary<string, User> Users =
            new Dictionary<string, User>();

        public static User Register(string username, string password,
          string[] roles = null)
        {
            if (Users.ContainsKey(username))
                return Users[username];

            // generate a random salt
            var rng = RandomNumberGenerator.Create();
            var saltBytes = new byte[16];
            rng.GetBytes(saltBytes);
            var saltText = Convert.ToBase64String(saltBytes);

            // generate the salted and hashed password 
            var saltedhashedPassword = SaltAndHashPassword(
              password, saltText);

            var user = new User
            {
                Name = username,
                Salt = saltText,
                SaltedHashedPassword = saltedhashedPassword,
                Roles = roles
            };
            Users.Add(user.Name, user);

            return user;
        }

        public static void LogIn(string username, string password)
        {
            if (CheckPassword(username, password))
            {
                var identity = new GenericIdentity(username, "PacktAuth");
                var principal = new GenericPrincipal(
                  identity, Users[username].Roles);

                System.Threading.Thread.CurrentPrincipal = principal;
            }
        }

        public static bool CheckPassword(string username, string password)
        {
            if (!Users.ContainsKey(username))
            {
                return false;
            }
            var user = Users[username];

            // re-generate the salted and hashed password 
            var saltedhashedPassword = SaltAndHashPassword(
              password, user.Salt);

            return (saltedhashedPassword == user.SaltedHashedPassword);
        }

        private static string SaltAndHashPassword(string password, string salt)
        {
            var sha = SHA256.Create();
            var saltedPassword = password + salt;
            return Convert.ToBase64String(
              sha.ComputeHash(Encoding.Unicode.GetBytes(saltedPassword)));
        }
    }
}