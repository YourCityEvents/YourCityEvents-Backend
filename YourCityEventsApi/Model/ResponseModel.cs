using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace YourCityEventsApi.Model
{
    public class ResponseModel<Type> 
    {
        public Dictionary<string,Type> Data { get; set; }
        
        public bool Result { get; set; }
        
        public IEnumerable<string> Errors { get; set; }
        
        public ResponseModel (Dictionary<string,Type> data, bool result=true, IEnumerable<string> errors=null)
        {
            Data = data;
            Result = result;
            Errors = errors;
        }

        public static ResponseModel<Type> FormResponse(string key,Type dictionaryData
            ,string errorMessage)
        {
            if (dictionaryData != null)
            {
                var data = new Dictionary<string, Type>();
                data.Add(key, dictionaryData);
                return new ResponseModel<Type>(data);
            }

            return new ResponseModel<Type>(null,false,new []{errorMessage});
        }
    }
}