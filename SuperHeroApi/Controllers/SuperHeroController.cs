using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuperHeroApi.Models;

namespace SuperHeroApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SuperHeroController : ControllerBase
    {
        private readonly DataContext _dataContext;

        public SuperHeroController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        [HttpGet("get")]
        public async Task<ActionResult<List<SuperHero>>> Get()
        {
            return Ok(await _dataContext.SuperHeroes.ToListAsync());
        }
        
        [HttpPost("create")]
        public async Task<ActionResult<List<SuperHero>>> Create(SuperHero hero)
        {
            _dataContext.SuperHeroes.Add(hero);
            await _dataContext.SaveChangesAsync();
            return Ok(await _dataContext.SuperHeroes.ToListAsync());
        }

        [HttpGet("getbyid/{id}")]
        public async Task<ActionResult<SuperHero>> GetById(Guid id)
        {
            var hero = await _dataContext.SuperHeroes.FindAsync(id);

            if(hero != null)
                return Ok(hero);
            return NotFound("Not Found");

        }

        [HttpPut("update")]
        public async Task<ActionResult<List<SuperHero>>> Update(SuperHero superHero)
        {
            var hero = await _dataContext.SuperHeroes.FindAsync(superHero.Id);

            if (hero != null)
            {
                hero.Place = superHero.Place;
                hero.Name = superHero.Name;
                hero.LastName = superHero.LastName;
                hero.FirstName = superHero.FirstName;

                await _dataContext.SaveChangesAsync();

                return Ok(await _dataContext.SuperHeroes.ToListAsync());
            } 
            return NotFound("Not Found");

        }

        [HttpDelete("delete/{id}")]
        public async Task<ActionResult<List<SuperHero>>> Delete(Guid id)
        {
            var hero = await _dataContext.SuperHeroes.FindAsync(id);

            if (hero == null)
                return NotFound("Not Found");
            
            _dataContext.SuperHeroes.Remove(hero);
            await _dataContext.SaveChangesAsync();
            return Ok();

        }
    }
}
