using JwtAuth.Dtos;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace JwtAuth.Services
{
    public class GoHighLevelService : IGoHighLevelService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public GoHighLevelService(
            HttpClient httpClient,
            IConfiguration configuration
        )
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string?> UpsertContactAsync(
            string firstName,
            string lastName,
            string email,
            string? phone
        )
        {
            string? token =
                _configuration["GoHighLevel:PrivateIntegrationToken"];

            string? locationId =
                _configuration["GoHighLevel:LocationId"];

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new InvalidOperationException(
                    "GoHighLevel token is missing."
                );
            }

            if (string.IsNullOrWhiteSpace(locationId))
            {
                throw new InvalidOperationException(
                    "GoHighLevel Location ID is missing."
                );
            }

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://services.leadconnectorhq.com/contacts/upsert"
            );

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token
                );

            request.Headers.Add("Version", "v3");

            request.Content = JsonContent.Create(new
            {
                firstName,
                lastName,
                email,
                phone,
                locationId
            });

            var response = await _httpClient.SendAsync(request);

            string responseBody =
                await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"GoHighLevel contact upsert failed: {responseBody}"
                );
            }

            using JsonDocument document =
                JsonDocument.Parse(responseBody);

            return document.RootElement
                .GetProperty("contact")
                .GetProperty("id")
                .GetString();
        }

        public async Task<List<GoHighLevelContactDto>> SearchContactsAsync(
    string keyword
)
        {
            string? token =
                _configuration["GoHighLevel:PrivateIntegrationToken"];

            string? locationId =
                _configuration["GoHighLevel:LocationId"];

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new InvalidOperationException(
                    "GoHighLevel token is missing."
                );
            }

            if (string.IsNullOrWhiteSpace(locationId))
            {
                throw new InvalidOperationException(
                    "GoHighLevel Location ID is missing."
                );
            }

            if (string.IsNullOrWhiteSpace(keyword))
            {
                return [];
            }

            var contacts = new List<GoHighLevelContactDto>();

            string? startAfterId = null;
            long? startAfter = null;

            while (true)
            {
                var url =
                    "https://services.leadconnectorhq.com/contacts/" +
                    $"?locationId={Uri.EscapeDataString(locationId)}" +
                    $"&query={Uri.EscapeDataString(keyword.Trim())}" +
                    "&limit=100";

                if (!string.IsNullOrWhiteSpace(startAfterId))
                {
                    url +=
                        $"&startAfterId={Uri.EscapeDataString(startAfterId)}";
                }

                if (startAfter.HasValue)
                {
                    url += $"&startAfter={startAfter.Value}";
                }

                using var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    url
                );

                request.Headers.Authorization =
                    new AuthenticationHeaderValue(
                        "Bearer",
                        token
                    );

                request.Headers.Add(
                    "Version",
                    "2023-02-21"
                );

                var response =
                    await _httpClient.SendAsync(request);

                string responseBody =
                    await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException(
                        $"GoHighLevel contact search failed: {responseBody}"
                    );
                }

                using JsonDocument document =
                    JsonDocument.Parse(responseBody);

                JsonElement root =
                    document.RootElement;

                if (!root.TryGetProperty(
                    "contacts",
                    out JsonElement contactsElement))
                {
                    break;
                }

                var pageContacts =
                    contactsElement.EnumerateArray().ToList();

                foreach (JsonElement contact in pageContacts)
                {
                    contacts.Add(new GoHighLevelContactDto
                    {
                        Id =
                            contact.TryGetProperty("id", out var id)
                                ? id.GetString() ?? ""
                                : "",

                        FirstName =
                            contact.TryGetProperty(
                                "firstName",
                                out var firstName
                            )
                                ? firstName.GetString() ?? ""
                                : "",

                        LastName =
                            contact.TryGetProperty(
                                "lastName",
                                out var lastName
                            )
                                ? lastName.GetString() ?? ""
                                : "",

                        Email =
                            contact.TryGetProperty(
                                "email",
                                out var email
                            )
                                ? email.GetString()
                                : null,

                        Phone =
                            contact.TryGetProperty(
                                "phone",
                                out var phone
                            )
                                ? phone.GetString()
                                : null
                    });
                }

                if (pageContacts.Count < 100)
                {
                    break;
                }

                JsonElement lastContact =
                    pageContacts[^1];

                startAfterId =
                    lastContact.GetProperty("id").GetString();

                string? dateAdded =
                    lastContact.TryGetProperty(
                        "dateAdded",
                        out var dateAddedElement
                    )
                        ? dateAddedElement.GetString()
                        : null;

                if (string.IsNullOrWhiteSpace(dateAdded))
                {
                    break;
                }

                startAfter =
                    new DateTimeOffset(
                        DateTime.Parse(dateAdded)
                    ).ToUnixTimeMilliseconds();
            }

            return contacts;
        }

        public async Task<GoHighLevelContactDto?> GetContactAsync(string contactId)
        {
            string? token =
                _configuration["GoHighLevel:PrivateIntegrationToken"];

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new InvalidOperationException(
                    "GoHighLevel token is missing."
                );
            }

            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://services.leadconnectorhq.com/contacts/{contactId}"
            );

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token
                );

            request.Headers.Add("Version", "v3");

            var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            string responseBody =
                await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"GoHighLevel contact lookup failed: {responseBody}"
                );
            }

            using JsonDocument document =
                JsonDocument.Parse(responseBody);

            JsonElement contact =
                document.RootElement.GetProperty("contact");

            return new GoHighLevelContactDto
            {
                Id = contact.GetProperty("id").GetString() ?? "",

                FirstName =
                    contact.TryGetProperty("firstName", out var firstName)
                        ? firstName.GetString() ?? ""
                        : "",

                LastName =
                    contact.TryGetProperty("lastName", out var lastName)
                        ? lastName.GetString() ?? ""
                        : "",

                Email =
                    contact.TryGetProperty("email", out var email)
                        ? email.GetString()
                        : null,

                Phone =
                    contact.TryGetProperty("phone", out var phone)
                        ? phone.GetString()
                        : null
            };
        }

        public async Task<string?> SendEmailAsync(
    string contactId,
    string subject,
    string html
)
        {
            string? token =
                _configuration["GoHighLevel:PrivateIntegrationToken"];

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new InvalidOperationException(
                    "GoHighLevel token is missing."
                );
            }

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://services.leadconnectorhq.com/conversations/messages"
            );

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token
                );

            request.Headers.Add("Version", "v3");

            request.Content = JsonContent.Create(new
            {
                type = "Email",
                contactId,
                subject,
                html,
                status = "pending"
            });

            var response = await _httpClient.SendAsync(request);

            string responseBody =
                await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"GoHighLevel email send failed: {responseBody}"
                );
            }

            using JsonDocument document =
                JsonDocument.Parse(responseBody);

            return document.RootElement
                .GetProperty("messageId")
                .GetString();
        }

        public async Task<string?> SendSmsAsync(
    string contactId,
    string message
)
        {
            string? token =
                _configuration["GoHighLevel:PrivateIntegrationToken"];

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new InvalidOperationException(
                    "GoHighLevel token is missing."
                );
            }

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://services.leadconnectorhq.com/conversations/messages"
            );

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token
                );

            request.Headers.Add("Version", "v3");

            request.Content = JsonContent.Create(new
            {
                type = "SMS",
                contactId,
                message,
                status = "pending"
            });

            var response = await _httpClient.SendAsync(request);

            string responseBody =
                await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"GoHighLevel SMS send failed: {responseBody}"
                );
            }

            using JsonDocument document =
                JsonDocument.Parse(responseBody);

            return document.RootElement
                .GetProperty("messageId")
                .GetString();
        }
    }
}