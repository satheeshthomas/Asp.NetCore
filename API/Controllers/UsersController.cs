using Microsoft.AspNetCore.Mvc;
using API.Data;
using API.Entities;
using System.Collections.Generic;
using System.Collections;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController:ControllerBase
    {
        private readonly DataContext _context;
        public UsersController(DataContext context){
            _context=context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers(){
            return await  _context.user.ToListAsync();              
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id){
            return await _context.user.FindAsync(id);
            
        }
    }
}