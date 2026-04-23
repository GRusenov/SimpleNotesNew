using System;
using System.Net;
using System.Text.Json;
using RestSharp;
using RestSharp.Authenticators;
using SimpleNotes.Models;

namespace SimpleNotes
{
    [TestFixture]
    public class Tests
    {
        private RestClient client;
        private static string CreatedNoteId;


        private const string BaseUrl = "http://144.91.123.158:5005";
        private const string StaticToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJKd3RTZXJ2aWNlQWNjZXNzVG9rZW4iLCJqdGkiOiIxZmYwYTFhYS04M2MzLTQxZDctODA3MC1kOWE5ODViNzEwYjMiLCJpYXQiOiIwNC8yMy8yMDI2IDA4OjQ0OjU1IiwiVXNlcklkIjoiMGQ2M2I5MjItYjdhNC00Nzg0LWFhNWEtNTk0OTA1MTcxYjQ2IiwiRW1haWwiOiJ0ZXN0NUBzb2Z0dW5pLmJnIiwiVXNlck5hbWUiOiJUZXN0ZXI1MyIsImV4cCI6MTc3Njk1NTQ5NSwiaXNzIjoiU2ltcGxlTm90ZXNfQXBwX1NvZnRVbmkiLCJhdWQiOiJTaW1wbGVOb3Rlc19XZWJBUElfU29mdFVuaSJ9.yCExAsHjp9_Vyt8xlewjm9eYBgnzalaAivUTYCyPx04";

        private const string LoginEmail = "test5@softuni.bg";
        private const string LoginPassword = "123456";

       

        [OneTimeSetUp]
        
        public void Setup()
        {
            var jwtToken = GetJwtToken(LoginEmail, LoginPassword);

            var options = new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };

            this.client = new RestClient(options);
        }

        private string GetJwtToken(string email, string password)
        {
            var tempClient = new RestClient(BaseUrl);
            var request = new RestRequest("api/User/Authorization", Method.Post);

            request.AddJsonBody(new { email, password });

            var response = tempClient.Execute(request);

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            return json.GetProperty("accessToken").GetString();
        }

        [Order(1)]
        [Test]
        public void CreateNote_WithoutRequiredFields()
        {
            var request = new RestRequest("/api/Note/Create", Method.Post);
            request.AddJsonBody(new { });

            var response = this.client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        }
        [Order(2)]
        [Test]
         public void CreateNote_WithValidData()
        {
            var request = new RestRequest("/api/Note/Create", Method.Post);
            request.AddJsonBody(new
            {
                title = "Test Note",
                description = "asdfghjklzxcvbnmlkjhgfdsaqwerty",
                status = "New"

            });
            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var createResponse = JsonSerializer.Deserialize<JsonElement>(response.Content);
            Assert.That(createResponse.GetProperty("msg").GetString(),
                        Is.EqualTo("Note created successfully!"));

        }
        [Order(3)]
        [Test]
        public void GetAllNotes_ShouldReturnNotes()
        {
           
            
                var request = new RestRequest("/api/Note/AllNotes", Method.Get);

                var response = this.client.Execute(request);

                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

                var notes = JsonSerializer.Deserialize<List<NoteDTO>>(
                    JsonDocument.Parse(response.Content)
                        .RootElement
                        .GetProperty("allNotes")
                        .GetRawText()
                );

               


            CreatedNoteId = notes.Last().Id;

            Assert.That(CreatedNoteId, Is.Not.Null.Or.Empty);
            }
        [Order(4)]
        [Test]
        public void EditExcistingNote_ShouldReturnSuccess()
        {
            var editRequestData = new NoteDTO();
            var request = new RestRequest($"/api/Note/Edit/{CreatedNoteId}", Method.Put);
            
           


            request.AddJsonBody(new
            {
                title = "Updated Note Title",
                description = "This is an updated description that is definitely longer than thirty characters.",
                status = "Done"
            });

            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var editResponse = JsonSerializer.Deserialize<JsonElement>(response.Content);
            Assert.That(editResponse.GetProperty("msg").GetString(),
                        Is.EqualTo("Note edited successfully!"));
        }
        [Order(5)]
        [Test]
        public void DeleteExistingNote_ShouldReturnSuccess()
        {
            var request = new RestRequest($"/api/Note/Delete/{CreatedNoteId}", Method.Delete);

            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var deleteResponse = JsonSerializer.Deserialize<JsonElement>(response.Content);
            Assert.That(deleteResponse.GetProperty("msg").GetString(),
                        Is.EqualTo("Note deleted successfully!"));
        }






        [OneTimeTearDown]
        public void TearDown()
        {
            this.client?.Dispose();
        }
    }
}