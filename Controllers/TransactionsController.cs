using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using PaymentAPI.Data;
using PaymentAPI.Models;
using PaymentAPI.Services;
using System.Text.Json;

namespace PaymentAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AccountVerificationService _verificationService;

        public TransactionsController(AppDbContext context, AccountVerificationService verificationService)
        {
            _context = context;
            _verificationService = verificationService;
        }

        [HttpPost("verify-account")]
        public async Task<ActionResult> VerifyAccount([FromBody] JsonElement request)
        {
            string accountNumber = request.GetProperty("accountNumber").GetString();
            string bankCode = request.GetProperty("bankCode").GetString();

            var result = await _verificationService.GetAccountDetails(accountNumber, bankCode);

            if (!result.IsValid)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [Authorize]
        [HttpPost("transfer")]
        public async Task<ActionResult> ProcessTransfer(TransferRequest request)
        {
            
            if (request.Amount <= 0)
            {
                return BadRequest(new { message = "Amount must be greater than zero" });
            }

            if (string.IsNullOrWhiteSpace(request.SourceAccountNumber))
            {
                return BadRequest(new { message = "Source account number is required" });
            }

            if (string.IsNullOrWhiteSpace(request.DestinationAccountNumber))
            {
                return BadRequest(new { message = "Destination account number is required" });
            }

            if (request.SourceAccountNumber == request.DestinationAccountNumber)
            {
                return BadRequest(new { message = "Cannot transfer to the same account" });
            }

            
            using var dbTransaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                
                var sourceAccount = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.AccountNumber == request.SourceAccountNumber);
                
                var destinationAccount = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.AccountNumber == request.DestinationAccountNumber);

                
                if (sourceAccount == null)
                {
                    return NotFound(new { message = "Source account not found" });
                }

                if (destinationAccount == null)
                {
                    return NotFound(new { message = "Destination account not found" });
                }

                
                if (sourceAccount.Balance < request.Amount)
                {
                    return BadRequest(new { message = "Insufficient funds" });
                }

                
                var transaction = new Transaction
                {
                    TransactionReference = GenerateTransactionReference(),
                    SourceAccountNumber = request.SourceAccountNumber,
                    DestinationAccountNumber = request.DestinationAccountNumber,
                    Amount = request.Amount,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };

                _context.Transactions.Add(transaction);

                sourceAccount.Balance -= request.Amount;

                destinationAccount.Balance += request.Amount;
          
                transaction.Status = "Success";
              
                await _context.SaveChangesAsync();

                await dbTransaction.CommitAsync();

                return Ok(new
                {
                    message = "Transfer successful",
                    transactionReference = transaction.TransactionReference,
                    sourceBalance = sourceAccount.Balance,
                    destinationBalance = destinationAccount.Balance
                });
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();

                return StatusCode(500, new
                {
                    message = "Transfer failed",
                    error = ex.Message
                });
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<Transaction>>> GetAllTransactions()
        {
            var transactions = await _context.Transactions
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
            return Ok(transactions);
        }

        [HttpGet("account/{accountNumber}")]
        public async Task<ActionResult<List<Transaction>>> GetAccountTransactions(string accountNumber)
        {
            var transactions = await _context.Transactions
                    .Where(t => t.SourceAccountNumber == accountNumber || 
                                t.DestinationAccountNumber == accountNumber)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();
    
            return Ok(transactions);
        }

        [HttpGet("{transactionReference}")]
        public async Task<ActionResult<Transaction>> GetTransactionByReference(string transactionReference)
        {
            var transactions = await _context.Transactions
                .FirstOrDefaultAsync(t => t.TransactionReference == transactionReference);

            if (transactions == null)
            {
                return NotFound(new { message = "Transaction not found" });
            }

            return Ok(transactions);
        }
    



        private string GenerateTransactionReference()
        {
            return $"TXN{DateTime.UtcNow:yyyyMMddHHmmss}";
        }
    }
}