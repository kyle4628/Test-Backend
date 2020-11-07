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
    //[JwtAuthActionFilter]
    [RoutePrefix("map")]
    [EnableCors(origins: "http://localhost:8080", headers: "*", methods: "*", SupportsCredentials = true)]
    public class MapController : ApiController
    {
        FUENMLEntities db = new FUENMLEntities();

        [Route("get_marks")]
        [HttpPost]
        [EnableCors("*", "*", "*")]
        public HttpResponseMessage getMarkInfo(tGmap mapAOI)
        {
            var areaId = db.places.Where(p => (double)p.longitude > mapAOI.from.lon && (double)p.longitude < mapAOI.to.lon &&
                                              (double)p.latitude > mapAOI.from.lat && (double)p.latitude < mapAOI.to.lat)
                                              .Select(p => p.id).ToList();
            List<tMapMark> Marks = new List<tMapMark>();
            List<string> systemTagResult = new List<string>();
            List<tTag> resultTagInfo = new List<tTag>();
            List<int> intersectResult = new List<int>();
            List<int> tagsList = new List<int>();
            int[] tFilterId = mapAOI.filter;
            int userlogin = 0;
            var mapInfo = new
            {
                marks = Marks,
                systemtags = systemTagResult,
                user_tags = resultTagInfo
            };
            var result = new
            {
                status = 0,
                data = mapInfo,
                msg = "fail"
            };

            //get intersect tagId by tags in filter
            intersectResult = areaId;
            if (tFilterId != null && tFilterId.Length > 0)
            {
                foreach (int i in tFilterId)
                {
                    intersectResult = tagFactory.searchTag(userlogin, ref intersectResult, i, db);
                }
                intersectResult = intersectResult.Distinct().ToList();
            }

            //get place information by intersectResult or all tags filter
            if (intersectResult != null) 
            {
                foreach (int i in intersectResult)
                {
                    var placeInfo = db.places.FirstOrDefault(t => t.id == i);
                    tMapMark markItem = new tMapMark();
                    location location = new location();
                    markItem.place_id = placeInfo.id;
                    location.lat = placeInfo.latitude;
                    location.lon = placeInfo.longitude;
                    markItem.location = location;

                    tagsList.AddRange(db.tagRelationships.Where(p => p.place_id == i).Select(q => q.tag_id).ToList());
                    systemTagResult.Add(placeInfo.type);
                    Marks.Add(markItem);
                }
                tagsList = tagsList.Distinct().ToList();
                Array.Sort(tagsList.ToArray());
                systemTagResult = systemTagResult.Distinct().ToList();
                if (tagsList.Count > 0)
                {
                    foreach (int i in tagsList)
                    {
                        var tagModel = db.tags.FirstOrDefault(p => p.id == i && p.type == 2);
                        if (tagModel != null)
                        {
                            tTag tagItem = new tTag();
                            tagItem.id = tagModel.id;
                            tagItem.name = tagModel.name;
                            tagItem.type = tagModel.type;
                            resultTagInfo.Add(tagItem);
                        }
                    }
                }
                mapInfo = new
                {
                    marks = Marks,
                    systemtags = systemTagResult,
                    user_tags = resultTagInfo
                };
                result = new
                {
                    status = 1,
                    data = mapInfo,
                    msg = ""
                };
            }
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [Route("get_place_info")]
        [HttpPost]
        [EnableCors("*", "*", "*")]
        public HttpResponseMessage getGooglePlaces(tGMapId gMapId)
        {
            var placeItem = db.places.FirstOrDefault(p => p.id == gMapId.place_id);
            placeInfoData placeInfo = new placeInfoData();
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
                //placeInfo.phone = placeItem.photo.ToString();

                result = new
                {
                    status = 1,
                    data = placeInfo,
                    msg = ""
                };
            }
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // Remove before final released
        [Route("get_place_tags")]
        [HttpPost]
        [EnableCors("*", "*", "*")]
        public HttpResponseMessage getPlaceTag(tGMapId gMapId)
        {
            List<tTag> placeTags = new List<tTag>();
            List<tTag> myTags = new List<tTag>();
            List<int> usertags = new List<int>();
            int userlogin = 0;
            var placeId = db.places.FirstOrDefault(p => p.id == gMapId.place_id).id;
            var tagList = db.tagRelationships.Where(t => t.place_id == placeId).Select(p => p.tag_id).ToList();
            int[] item = tagList.Distinct().ToArray();
            userlogin = userFactory.userIsLoginSession(userlogin);
            var dataForm = new
            {
                my_tags = myTags,
                place_tags = placeTags
            };
            var result = new
            {
                status = 0,
                data = dataForm,
                msg = "fail"
            };
            if (userlogin != 0)
            {
                usertags = db.tagRelationships.Where(t => t.place_id == placeId && t.user_id == userlogin).Select(p => p.tag_id).ToList();
                if (usertags.Count > 0)
                {
                    foreach (int i in usertags)
                    {
                        tTag tagItem = new tTag();
                        var tag = db.tags.FirstOrDefault(t => t.id == i);
                        tagItem.id = tag.id;
                        tagItem.name = tag.name;
                        myTags.Add(tagItem);
                    }
                }
                dataForm = new
                {
                    my_tags = myTags,
                    place_tags = placeTags
                };
                result = new
                {
                    status = 1,
                    data = dataForm,
                    msg = ""
                };
            }
            if (tagList.Count > 0)
            {
                foreach (int i in item)
                {
                    tTag tagItem = new tTag();
                    var tag = db.tags.FirstOrDefault(t => t.id == i);
                    tagItem.id = tag.id;
                    tagItem.name = tag.name;
                    //tagItem.type = tag.type;
                    placeTags.Add(tagItem);

                    result = new
                    {
                        status = 1,
                        data = dataForm,
                        msg = ""
                    };
                }
            }
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }
    }
}
