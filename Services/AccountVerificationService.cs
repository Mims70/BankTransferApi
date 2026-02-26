using System.Text.Json;

namespace PaymentAPI.Services
{
    public class AccountVerificationService
    {
        private readonly HttpClient _httpClient;

        public AccountVerificationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> VerifyAccountNumber(string accountNumber, string bankCode)
        {
            try
            {
                var url = $"https://jsonplaceholder.typicode.com/users/{accountNumber}";

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    // If we get a response, account "exists"
                    return !string.IsNullOrEmpty(content);
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<AccountVerificationResult> GetAccountDetails(string accountNumber, string bankCode)
        {
            try
            {
                var url = $"https://jsonplaceholder.typicode.com/users/{accountNumber}";

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    return new AccountVerificationResult
                    {
                        IsValid = false,
                        Message = "Account not found"
                    };
                }

                var content = await response.Content.ReadAsStringAsync();
                var user = JsonSerializer.Deserialize<JsonElement>(content);

                return new AccountVerificationResult
                {
                    IsValid = true,
                    AccountName = user.GetProperty("name").GetString(),
                    AccountNumber = accountNumber,
                    BankCode = bankCode,
                    Message = "Account verified successfully"
                };
            }
            catch (Exception ex)
            {
                return new AccountVerificationResult
                {
                    IsValid = false,
                    Message = $"Verification failed: {ex.Message}"
                };
            }
        }
    }

    public class AccountVerificationResult
    {
        public bool IsValid { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public string BankCode { get; set; }
        public string Message { get; set; }
    }
}