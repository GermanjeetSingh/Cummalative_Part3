﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SchoolWebApp.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SchoolWebApp.Controllers
{
    [Route("api/[controller]")]
    public class TeacherDataController : Controller
    {
        private SchoolDbContext ctx = new SchoolDbContext();
        
        
        [Route("all")]
        public IActionResult Get()
        {
            var teachers = ctx.Teachers.AsEnumerable();
            return Ok(teachers);
        }

        [Route("search/{type}/{searchParam}")]
        public IActionResult Get(int type , string searchParam)
        {
            var teachers = Enumerable.Empty<Teacher>();
            if (type == 1)
            {
                teachers = ctx
                    .Teachers
                    .Where(teacher =>
                        teacher.teacherfname.ToLower().Equals(searchParam) ||
                        teacher.teacherlname.ToLower().Equals(searchParam)).AsEnumerable();
            }else if(type == 2)
            {
                teachers = ctx
                       .Teachers
                       .Where(teacher =>
                           teacher.salary.ToString().Equals(searchParam)).AsEnumerable();
            }
            else if(type == 3)
            {
                DateTime datetime;
                bool flag = DateTime.TryParseExact(searchParam , "yyyy-MM-dd", CultureInfo.InvariantCulture,
                       DateTimeStyles.None,
                       out datetime);
                if(flag)
                {
                    teachers = ctx
                       .Teachers
                       .Where(teacher =>
                           teacher.hiredate.Equals(datetime)).AsEnumerable();
                }
            }

            return teachers.Count() == 0 ? NotFound() : Ok(teachers);
        }

        [HttpGet("{id}")]   
        public IActionResult Get(int id)
        {
            var teacher = ctx.Teachers.Find(id);
            if (teacher != null)
            {
                var classes = ctx.Classes.Where(cl => cl.teacherid == teacher.teacherid);
                var tc = new TeacherClasses(teacher,null);
                if (classes == null)
                    tc.classes = Enumerable.Empty<Class>();
                else
                    tc.classes = classes.AsEnumerable<Class>();
                return Ok(tc);
            }
            else
                return NotFound();
        }

        [HttpPost]
        public IActionResult Post([FromBody]Teacher teacher)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest("Invalid Request");
            }

            if(ctx.Teachers.Where(t => t.employeenumber.Equals(teacher.employeenumber)).Any())
            {
                return BadRequest("Teacher with same employee number already exist");
            }
            ctx.Teachers.Add(teacher);
            ctx.SaveChanges();
            return Ok(teacher.teacherid);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            Teacher teacher = ctx.Teachers.Where(t => t.teacherid.Equals(id)).First();
            if (teacher != null)
            {
                
                ctx.Classes
                    .Where(_class => _class.teacherid == teacher.teacherid)
                    .ToList()
                    .ForEach(_class =>
                {
                    ctx.Classes.Remove(_class);
                });
                ctx.Teachers.Remove(teacher);
                ctx.SaveChanges();
                return Ok();
            }else
            {
                return NotFound();
            }
        }
    }
}
