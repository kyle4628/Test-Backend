﻿using prjToolist.Models;
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
    public class MapController : ApiController
    {
        FUENMLEntities db = new FUENMLEntities();

        [Route("get_marks")]
        [HttpPost]
        [EnableCors("*", "*", "*")]
        public HttpResponseMessage getMarkInfo(tGmap mapAOI)
        {
            var areaId = db.places.Where(p => (double)p.longitude > mapAOI.from.lon && (double)p.longitude < mapAOI.to.lon &&
                                             (double)p.latitude > mapAOI.from.lat && (double)p.latitude < mapAOI.to.lat
                                       ).Select(p => p.id).ToList();
            List<tMapMark> Marks = new List<tMapMark>();
            List<int> intersectResult = new List<int>();
            int[] tFilterid = mapAOI.filter;
            int userlogin = 0;
            var result = new
            {
                status = 1,
                data = Marks,
                msg = "fail"
            };
            intersectResult = areaId;
            if (tFilterid != null && tFilterid.Length > 0)
            {

                foreach (int i in tFilterid)
                {
                    intersectResult = tagFactory.searchTag(userlogin, ref intersectResult, i, db);
                }
                intersectResult = intersectResult.Distinct().ToList();
            }

            if (intersectResult != null)
            {
                foreach (int i in intersectResult)
                {
                    var placeInfo = db.places.FirstOrDefault(t => t.id == i);
                    tMapMark markItem = new tMapMark();
                    location location = new location();
                    markItem.gmap_id = placeInfo.gmap_id;
                    location.lat = (float)placeInfo.latitude;
                    location.lon = (float)placeInfo.longitude;
                    markItem.location = location;

                    Marks.Add(markItem);
                }
                result = new
                {
                    status = 0,
                    data = Marks,
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
            var tagList = db.tagRelationships.Where(t => t.place_id == placeId).Select(p=> p.tag_id).ToList();
            List<tTag> tags = new List<tTag>();
            
            int[] item = tagList.Distinct().ToArray();

            var result = new
            {
                status = 0,
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
                    status = 1,
                    data = tags,
                    msg = ""
                };
            }
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [Route("candidate_tag")]
        [HttpPost]
        [EnableCors("*", "*", "*")]
        public HttpResponseMessage getCandidatePlace(candidatePlacePara candidatePlacePara)
        {
            int cadidatePlaceId = db.places.FirstOrDefault(p => p.gmap_id == candidatePlacePara.gmap_id).id;
            var cadidateTags = db.tagRelationships.Where(t => t.place_id == cadidatePlaceId)
                                              .Select(t => t.tag_id).ToList();

            int[] tagIdList = cadidateTags.ToArray();
            Array.Sort(tagIdList);
            List<tTag> tagList = new List<tTag>();

            var tagInfo = new
            {
                tags = tagList
            };

            var result = new
            {
                status = 0,
                data = tagInfo,
                msg = "fail"
            };

            if (tagIdList.Count() > 0)
            {
                foreach(int i in tagIdList)
                {
                    var tagModel = db.tags.Where(t => t.id == i).FirstOrDefault();
                    tTag tagItem = new tTag();
                    tagItem.id = tagModel.id;
                    tagItem.name = tagModel.name;
                    tagItem.type = tagModel.type;
                    tagList.Add(tagItem);
                }

                tagInfo = new
                {
                    tags = tagList
                };

                result = new
                {
                    status = 1,
                    data = tagInfo,
                    msg = ""
                };
            }
            
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }
    }
}
