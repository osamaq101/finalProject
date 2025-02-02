using Amazon.DynamoDBv2.DataModel;

namespace AuthApp.Models
{
    [DynamoDBTable("User")]
    public class User
    {
        [DynamoDBHashKey]
        public string Email { get; set; }
      
        [DynamoDBProperty]
        public string Name { get; set; }
       
        [DynamoDBProperty]
        public string PasswordHash { get; set; }
       
        [DynamoDBProperty]
        public string ProfileImageUrl { get; set; }
    }
}
