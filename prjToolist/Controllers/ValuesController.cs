using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using Newtonsoft.Json.Linq;
using prjToolist.Models;

namespace prjToolist.Controllers {
    [RoutePrefix("query")]
    public class ValuesController : ApiController {
        private readonly FUENMLEntities db = new FUENMLEntities();
        public int str { get; set; }

        [Route("get_user_list")]
        [HttpPost]
        [EnableCors("*", "*", "*")]
        public HttpResponseMessage getUserList()
        {
            var intList = db.users.Select(p => p.id).ToList();
            List<queryUserList> usersList = new List<queryUserList>();
            foreach (int i in intList)
            {
                var userListItem = db.users.FirstOrDefault(p => p.id == i);
                queryUserList listItem = new queryUserList();
                listItem.id = userListItem.id;
                listItem.name = userListItem.name;
                listItem.email = userListItem.email;
                listItem.authority = userListItem.authority;
                listItem.password = userListItem.password;
                listItem.createdTime = userListItem.created.ToString();
                listItem.updatedTime = userListItem.updated.ToString();
                usersList.Add(listItem);
            }
            var result = new
            {
                data = usersList,
                total = usersList.Count()
            };

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [Route("get_tag_list")]
        [HttpPost]
        [EnableCors("*", "*", "*")]
        public HttpResponseMessage getTagList()
        {
            var intList = db.tags.Select(p => p.id).ToList();
            List<tTag> tagsList = new List<tTag>();
            foreach (int i in intList)
            {
                var tagListItem = db.tags.FirstOrDefault(p => p.id == i);
                tTag listItem = new tTag();
                listItem.id = tagListItem.id;
                listItem.name = tagListItem.name;
                listItem.type = tagListItem.type;

                tagsList.Add(listItem);
            }
            var result = new
            {
                data = tagsList,
                total = tagsList.Count()
            };

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [Route("get_place_list")]
        [HttpPost]
        [EnableCors("*", "*", "*")]
        public HttpResponseMessage getPlaceList()
        {
            var intList = db.places.Select(p=>p.id).ToList();
            List<queryPlaceList> placesList = new List<queryPlaceList>();
            foreach (int i in intList)
            {
                var placeListItem = db.placeLists.FirstOrDefault(p => p.id == i);
                queryPlaceList listItem = new queryPlaceList();
                listItem.id = placeListItem.id;
                listItem.listName = placeListItem.name;
                listItem.description = placeListItem.description;
                listItem.privacy = placeListItem.privacy;
                listItem.user_id = placeListItem.user_id;
                listItem.createdTime = placeListItem.created.ToString();
                listItem.updatedTime = placeListItem.updated.ToString();
                placesList.Add(listItem);
            }
            var result = new
            {
                data = placesList,
                total = placesList.Count()
            };

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [HttpPost]
        [Route("listPost")]
        [EnableCors("*", "*", "*")]
        public IEnumerable<user> ttt() {
            //public List<Student> Get() {
            var api = from p in db.users
                select p;
            //user.Add*()
            return api.ToList();
        }

        [HttpPost]
        // POST api/values
        public HttpResponseMessage Post([FromBody] string createUser) {
            var controllerName = ControllerContext.RouteData.Values["controller"].ToString();
            var jo = JObject.Parse(createUser);
            var name = jo["name"].ToString();
            var email = jo["email"].ToString();
            var pwd = jo["password"].ToString();
            var createdTime = jo["created"].ToString();
            //string updatedTime = jo["updated"].ToString();
            var authorityNew = jo["authority"].ToString();
            var createMember = new user();
            createMember.name = name;
            createMember.email = email;
            createMember.password = pwd;
            createMember.created = DateTime.Parse(createdTime);
            //createMember.updated = DateTime.Parse(updatedTime);
            createMember.authority = int.Parse(authorityNew);
            db.users.Add(createMember);
            db.SaveChanges();
            var result = new {
                STATUS = true,
                MSG = "成功"
            };

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // PUT api/values/5
        public HttpResponseMessage Put(int id, [FromBody] string updateUser) {
            var controllerName = ControllerContext.RouteData.Values["controller"].ToString();
            var jo = JObject.Parse(updateUser);
            //string memberId = jo["id"].ToString();
            var name = jo["name"].ToString();
            var email = jo["email"].ToString();
            var pwd = jo["password"].ToString();
            var updatedTime = jo["updated"].ToString();
            var authorityNew = jo["authority"].ToString();
            var updateMember = db.users.FirstOrDefault(p => p.id == id);

            //user createMember = new user();
            updateMember.name = name;
            updateMember.email = email;
            updateMember.password = pwd;
            updateMember.updated = DateTime.Parse(updatedTime);
            updateMember.authority = int.Parse(authorityNew);
            db.SaveChanges();
            var result = new {
                STATUS = true,
                MSG = "成功"
            };

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        public class Student {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}