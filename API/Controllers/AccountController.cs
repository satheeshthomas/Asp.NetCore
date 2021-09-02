using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {

        private readonly DataContext _datacontext;
        private readonly ITokenService _tokenService;
        public AccountController(DataContext context,ITokenService tokenService){
            _datacontext=context;
            _tokenService= tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDTO)
        {
            if(await userExists(registerDTO.userName)) return BadRequest("Username is taken");

            using var hmac= new HMACSHA512();     

            var user = new User{
                UserName=registerDTO.userName,
                PasswordHash=hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.password)),
                PasswordHashSalt=hmac.Key
            };

            _datacontext.user.Add(user);  
            await _datacontext.SaveChangesAsync();  

            return new UserDTO{
                Username = user.UserName,
                token=_tokenService.CreateToken(user)
            };

             
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO)
        {
           var user = await _datacontext.user.SingleOrDefaultAsync(x=>x.UserName==loginDTO.userName);

           if(user==null) return Unauthorized("Invalid username");

           using var hmac= new HMACSHA512(user.PasswordHashSalt);
           var computeHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDTO.password));
           
           for(int i=0;i<computeHash.Length;i++){
               if(computeHash[i]!=user.PasswordHash[i]) return Unauthorized("Invalid password");
           }

            return new UserDTO{
                Username = user.UserName,
                token=_tokenService.CreateToken(user)
            };
        }
        private async Task<bool> userExists(string userName)
        {
          return await  _datacontext.user.AnyAsync(x=>x.UserName.ToLower()==userName.ToLower());
        }
    }
}