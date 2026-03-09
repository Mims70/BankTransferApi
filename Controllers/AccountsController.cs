using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using PaymentAPI.Data;
using PaymentAPI.Models;
using Microsoft.AspNetCore.Authorization;

namespace PaymentAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController : ControllerBase
    { 
        private readonly AppDbContext _context;

        public AccountsController(AppDbContext context)
        {
            _context = context;
        }

        
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Account>> CreateAccount(Account newAccount)
        { 
            if(string.IsNullOrWhiteSpace(newAccount.AccountName))            
            {
                return BadRequest("Account name is required.");
            }

            if (newAccount.Balance < 0)
            {
                return BadRequest(new { message = "Balance cannot be negative" });
            } 
            if (string.IsNullOrWhiteSpace(newAccount.AccountNumber))
            {
                return BadRequest(new { message = "Account number is required" });
            }

            if (newAccount.AccountNumber.Length != 10)
            {
                return BadRequest(new { message = "Account number must be exactly 10 digits" });
            }
            if  (!newAccount.AccountNumber.All(char.IsDigit))
            {
                return BadRequest(new { message = "Account number must contain only digits" });
            }
            
            var existingAccount = await _context.Accounts
                        .FirstOrDefaultAsync(a => a.AccountNumber == newAccount.AccountNumber);
    
            if (existingAccount != null)
            {
                return BadRequest(new { message = "Account number already exists" });
            }

            _context.Accounts.Add(newAccount);
            await _context.SaveChangesAsync();

            return Ok(newAccount);
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<Account>>> GetAllAccounts()
        {
            var accounts = await _context.Accounts.ToListAsync();
            return Ok(accounts);
        }

        [HttpGet("{accountNumber}")]
        public async Task<ActionResult<Account>> GetAccountByNumber(string accountNumber)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);
            if (account == null)
            {
                return NotFound();
            }
            return Ok(account);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAccount(int id)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }

            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}