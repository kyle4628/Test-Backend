﻿using prjToolist.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Http.Cors;
using static prjToolist.Models.tagFactory;
//using static prjToolist.Models.tTagRelation;

namespace prjToolist.Controllers
{
    [RoutePrefix("common")]
    //[JwtAuthActionFilter]
    public class CommonController : ApiController
    {
        FUENMLEntities db = new FUENMLEntities();

        [Route("get_recommend_lists")]
        [HttpPost]
        [EnableCors("*", "*", "*")]
        public HttpResponseMessage getRecommendList(tFilter filter)
        {
            List<tPlaceList> placeList = new List<tPlaceList>();
            List<int> intersectPlaceId = new List<int>();
            List<int> intersectPlaceListId = new List<int>();

            int[] tagIdList = db.tags.Select(t => t.id).ToArray();
            string[] typeList = db.places.Select(p => p.type).Distinct().ToArray();
            Array.Sort(tagIdList);
            List<tTag> tagList = new List<tTag>();
            foreach (int i in tagIdList)
            {
                var tagModel = db.tags.FirstOrDefault(t => t.id == i);
                tTag tagItem = new tTag();
                tagItem.id = i;
                tagItem.name = tagModel.name;
                tagList.Add(tagItem);
            }
            var tagInfo = new
            {
                lists = placeList,
                user_tags = tagList,
                system_tags = typeList
            };
            var result = new
            {
                status = 1,
                data = tagInfo,
                msg = ""
            };

            int userlogin = 0;
            var entirePlaces = db.places.Select(p => p.id).ToList();
            var entirePlaceLists = db.placeLists.Select(p => p.id).ToList();
            int filterCount = filter.filter.Length;
            intersectPlaceId = entirePlaces;
            intersectPlaceListId = entirePlaceLists;

            if (filterCount > 0)
            {
                foreach (int i in filter.filter)
                {
                    intersectPlaceId = tagFactory.searchTag(userlogin, ref intersectPlaceId, i, db);
                }
                Array.Sort(intersectPlaceId.Distinct().ToArray());
                foreach (int i in intersectPlaceId)
                {
                    var listFilter = db.placeRelationships.Where(p => p.place_id == i).Select(p => p.placelist_id).ToList();
                    intersectPlaceListId = intersectPlaceListId.Intersect(listFilter).ToList();
                }
                Array.Sort(intersectPlaceListId.Distinct().ToArray());
                foreach(int i in intersectPlaceListId)
                {
                    var placeListModel = db.placeLists.FirstOrDefault(p => p.id == i);
                    tPlaceList placeListItem = new tPlaceList();
                    placeListItem.id = placeListModel.id;
                    placeListItem.name = placeListModel.name;
                    //placeListItem.coverImageURL = placeListModel.cover.ToString();
                    placeList.Add(placeListItem);
                }

                tagInfo = new
                {
                    lists = placeList,
                    user_tags = tagList,
                    system_tags = typeList
                };
                result = new
                {
                    status = 1,
                    data = tagInfo,
                    msg = ""
                };
            }
            else
            {
                foreach(int i in entirePlaceLists)
                {
                    var placeListModel = db.placeLists.FirstOrDefault(p => p.id == i);
                    tPlaceList placeListItem = new tPlaceList();
                    placeListItem.id = placeListModel.id;
                    placeListItem.name = placeListModel.name;
                    //placeListItem.coverImageURL = placeListModel.cover.ToString();
                    placeList.Add(placeListItem);
                }
                tagInfo = new
                {
                    lists = placeList,
                    user_tags = tagList,
                    system_tags = typeList
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

        [Route("get_hot_tags")]
        [HttpGet]
        [EnableCors("*", "*", "*")]
        public HttpResponseMessage getHotTag()
        {
            int[] idList = db.tags.Select(t => t.id).ToArray();
            string[] typeList = db.places.Select(p => p.type).Distinct().ToArray();
            Array.Sort(idList); 
            List<tTag> tagList = new List<tTag>();
            var tagInfo = new
            {
                user_tags = tagList,
                system_tags = typeList
            };
            var result = new
            {
                status = 0,
                data = tagInfo,
                msg = "fail"
            };

            if (idList.Count() > 0)
            {
                foreach(int i in idList)
                {
                    var tagModel = db.tags.FirstOrDefault(t => t.id == i);
                    tTag tagItem = new tTag();
                    tagItem.id = i;
                    tagItem.name = tagModel.name;
                    //tagItem.type = tagModel.type;
                    tagList.Add(tagItem);
                    tagInfo = new
                    {
                        user_tags = tagList,
                        system_tags = typeList
                    };
                    result = new
                    {
                        status = 1,
                        data = tagInfo,
                        msg = ""
                    };
                }
            }
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }
        

        [Route("get_list")]
        [HttpPost]
        [EnableCors("*", "*", "*")]
        public HttpResponseMessage GetList(int list_id, tagString s)
        {
            //獲取某特定清單中含有標籤的地點id
            List<int> resultplaceid = new List<int>();
            int[] tFilterid = tagFactory.checktagString(s, db);
            var searchallplaceinlist = db.placeRelationships.Where(p => p.placelist_id == list_id)
                .Select(q => q.place_id).ToList();
            //此結果為特定清單中篩選出的地點id
            List<int> intersectResult = searchallplaceinlist;
            if (tFilterid != null)
            {
                foreach (int i in tFilterid)
                {
                    var searchplacehastag = db.tagRelationships.Where(P => P.tag_id == i).Select(q => q.place_id).ToList();
                    intersectResult = intersectResult.Intersect(searchplacehastag).ToList();
                    //foreach(int p in unionResult) {
                    //    resultplaceid.Add(p);
                    //}
                }
            }
            //============================================

            var place = db.placeLists.Where(p => p.id == list_id).FirstOrDefault();
            var placeSpot = db.placeRelationships.Where(p => p.placelist_id == list_id).Select(p => p.place_id).ToList();
            List<placeListInfo> infoList = new List<placeListInfo>();
            List<tPlaceInfo> relationPlace = new List<tPlaceInfo>();
            List<tagInfo> tagInfoList = new List<tagInfo>();

            List<int> idList = new List<int>();
            List<int> tagList = new List<int>();

            for (int i = 0; i < placeSpot.Count(); i++)
            {
                idList.Add(placeSpot[i]);
            }
            int[] terms = idList.ToArray();

            placeListInfo infoItem = new placeListInfo();
            infoItem.userId = place.user_id;
            infoItem.name = place.name;
            infoItem.description = place.description;
            infoItem.privacy = place.privacy;
            infoItem.createdTime = place.created.ToString();
            infoItem.updatedTime = place.updated.ToString();
            //byte[] binaryString = (byte[])place.cover;
            //info.cover = Encoding.UTF8.GetString(binaryString);
            infoList.Add(infoItem);

            foreach (var i in terms)
            {
                var tagId = db.tagRelationships.Where(p => p.place_id == i).Select(p => p.tag_id).ToList();
                tPlaceInfo exportPlaceInfo = new tPlaceInfo();
                var placeModel = db.places.FirstOrDefault(p => p.id == i);
                for (int j = 0; j < tagId.Count(); j++)
                {
                    tagList.Add(tagId[j]);
                }
                exportPlaceInfo.name = placeModel.name;
                exportPlaceInfo.longitude = placeModel.longitude;
                exportPlaceInfo.latitude = placeModel.latitude;
                exportPlaceInfo.phone = placeModel.phone;
                exportPlaceInfo.address = placeModel.address;
                exportPlaceInfo.type = placeModel.type;
                exportPlaceInfo.gmap_id = placeModel.gmap_id;
                relationPlace.Add(exportPlaceInfo);
            }

            int[] tagArray = tagList.Distinct().ToArray();

            foreach (int t in tagArray)
            {
                var exportTagInfo = new tagInfo();
                var tagInfoModel = db.tags.FirstOrDefault(p => p.id == t);
                exportTagInfo.name = tagInfoModel.name;
                exportTagInfo.type = tagInfoModel.type;
                tagInfoList.Add(exportTagInfo);
            }

            var dataForm = new
            {
                info = infoList,
                places = relationPlace,
                tags = tagInfoList
            };

            var result = new
            {
                status = 1,
                data = dataForm,
                msg = "",
            };

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [Route("set_tag_event")]
        [HttpPost]
        [EnableCors("*", "*", "*")]
        public HttpResponseMessage setEvent(tTagEvent tTagEvent)
        {
            var result = new
            {
                status = 0,
                msg = "invalid event type",
            };
            if (tTagEvent.tagEvent == 1 || tTagEvent.tagEvent == 2)
            {
                tagEvent newEvent = new tagEvent();
                newEvent.tag_id = tTagEvent.tag_id;
                newEvent.user_id = tTagEvent.user_id;
                newEvent.tagEvent1 = tTagEvent.tagEvent;
                newEvent.created = DateTime.Now;
                db.tagEvents.Add(newEvent);
                db.SaveChanges();

                result = new
                {
                    status = 1,
                    msg = "",
                };
            }
            
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [Route("tag_relation")]
        [HttpPost]
        [EnableCors("*", "*", "*")]
        public HttpResponseMessage tagRelation(tTagRelation tTag)
        {
            var verifyAccount = db.users.Where(u => u.id == tTag.user_id).FirstOrDefault();
            var verifyGlePlace = db.places.Where(p => p.gmap_id == tTag.gmap_id).FirstOrDefault();

            var result = new
            {
                status = 0,
                msg = "fail"
            };
            if (verifyAccount != null)
                if (verifyGlePlace != null)
                {
                    foreach (var i in tTag.tag_id)
                    {
                        var newTagRelation = new tagRelationship();
                        newTagRelation.place_id = verifyGlePlace.id;
                        newTagRelation.user_id = verifyAccount.id;
                        newTagRelation.tag_id = i;
                        newTagRelation.created = DateTime.Now;
                        db.tagRelationships.Add(newTagRelation);
                    }

                    db.SaveChanges();
                    result = new
                    {
                        status = 1,
                        msg = ""
                    };
                }

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }
    }
}
