using System.Collections.Generic;


namespace Messenger.Core.Models
{
    /// <summary>
    /// The source code side representation of a User
    /// </summary>
    public class User
    {
        /// <summary>
        /// The unique id of a user(pulled form microsoft services)
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The display name of the user
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// The id used to deduplicate user names(e.g. UserXY#1, UserXY#2)
        /// </summary>
        public uint NameId { get; set; }

        /// <summary>
        /// The path to the user's profile photo in the blob storage
        /// </summary>
        public string Photo { get; set; }

        /// <summary>
        /// The email address of the user
        /// </summary>
        public string Mail { get; set; }

        /// <summary>
        /// The user's bio
        /// </summary>
        public string Bio { get; set; }

        public User()
        {
            Id = "";
            DisplayName = "";
            Photo = "";
            Mail = "";
            Bio = "";
        }

        /// <summary>
        /// Default initialize all members(string values get initialized to "" instead of null)
        /// </summary>
        public override string ToString()
        {
            return $"User: Id={Id}, DisplayName={DisplayName}, NameId={NameId}, Photo={Photo}, Mail={Mail}, Bio={Bio}";
        }

        // TODO: Cleanup
        public List<string> Userlist { get; set; }
    }
}
