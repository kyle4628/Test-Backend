using prjToolist.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;

namespace prjToolist.Controllers
{
    [RoutePrefix("map")]
    public class MapController : ApiController
    {
        FUENMLEntities db = new FUENMLEntities();

        [Route("get_marks")]
        [HttpPost]
        [EnableCors("*", "*", "*")]
        public HttpResponseMessage getMarkInfo()
        {
            var result = new
            {
                status = 0,
                msg = "fail"
            };
            

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [Route("get_place_info")]
        [HttpPost]
        [EnableCors("*", "*", "*")]
        public HttpResponseMessage getGooglePlaces(tGMapId gMapId)
        {
            var placeItem = db.places.FirstOrDefault(p => p.gmap_id == gMapId.gmap_id);
            placeInfo placeInfo = new placeInfo();
            var result = new
            {
                status = 0,
                data = placeInfo,
                msg = "fail"
            };
            if(placeItem != null)
            {
                placeInfo.name = placeItem.name;
                placeInfo.phone = placeItem.phone;
                placeInfo.address = placeItem.address;
                placeInfo.type = placeItem.type;

                result = new
                {
                    status = 1,
                    data = placeInfo,
                    msg = ""
                };
            }


            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [Route("get_place_tag")]
        [HttpPost]
        [EnableCors("*", "*", "*")]
        public HttpResponseMessage getPlaceTag(tGMapId gMapId)
        {
            var placeId = db.places.FirstOrDefault(p => p.gmap_id == gMapId.gmap_id).id;
            var tagList = db.tagRelations.Where(t => t.place_id == placeId).Select(p=> p.tag_id).ToList();
            List<tTag> tags = new List<tTag>();
            
            int[] item = tagList.Distinct().ToArray();

            var result = new
            {
                status = 1,
                data = tags,
                msg = "fail"
            };
            foreach(int i in item)
            {
                tTag tagItem = new tTag();
                var tag = db.tags.FirstOrDefault(t => t.id == i);
                tagItem.id = tag.id;
                tagItem.name = tag.name;
                tagItem.type = tag.type;
                tags.Add(tagItem);

                result = new
                {
                    status = 0,
                    data = tags,
                    msg = ""
                };
            }
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }
    }
}
