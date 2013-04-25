using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Net;
using System.Xml;
using System.IO;


namespace Shipper_Tracker
{
    class UPS
    {

        public class TrackingInfo
        {
            public bool success;

            public string errorDescription;
            public string errorCode;

            public string trackingNumber;
            public string url;

            public bool pickedUp = false;
            public DateTime deliveryDate;

            public bool delivered = false;
            public DateTime pickedupdate;

            public string status;
            public string statusCode;
            public string service;

            public string shipFromAddress;
            public string shipFromCity;
            public string shipFromState;
            public string shipFromZip;
            public string shipFromCountry;

            public string shipToCity;
            public string shipToState;
            public string shipToZip;
            public string shipToCountry;

        }


        public static TrackingInfo TrackPackage(string trackingnumber)
        {

            TrackingInfo info = new TrackingInfo();


            string request =
                "<?xml version=\"1.0\" ?>"
                + "<AccessRequest xml:lang='en-US'>"
                    + "<AccessLicenseNumber>????????</AccessLicenseNumber>"
                    + "<UserId>?????????</UserId>"
                    + "<Password>????????</Password>"
                + "</AccessRequest>"
                + "<?xml version=\"1.0\" ?>"
                + "<TrackRequest>"
                    + "<Request>"
                        + "<TransactionReference>"
                            + "<CustomerContext>guidlikesubstance</CustomerContext>"
                        + "</TransactionReference>"
                        + "<RequestAction>Track</RequestAction>"
                    + "</Request>"
                    + "<TrackingNumber>" + trackingnumber + "</TrackingNumber>"
                + "</TrackRequest>";

            //string url = "https://wwwcie.ups.com/ups.app/xml/Track"; // test
            string url = "https://onlinetools.ups.com/ups.app/xml/Track"; // production 

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            
            byte[] requestBytes = System.Text.Encoding.ASCII.GetBytes(request);

            req.Method = "POST";
            req.ContentType = "text/xml;charset=utf-8";
            req.ContentLength = requestBytes.Length;

            Stream requestStream = req.GetRequestStream();
            requestStream.Write(requestBytes, 0, requestBytes.Length);
            requestStream.Close();

            HttpWebResponse res = (HttpWebResponse)req.GetResponse();

            XmlDocument doc = new XmlDocument();

            doc.Load(res.GetResponseStream());

                        
            string response = doc.GetValue("/TrackResponse/Response/ResponseStatusCode");

            info.url = "http://wwwapps.ups.com/WebTracking/processInputRequest?TypeOfInquiryNumber=T&loc=en_US&InquiryNumber=" + trackingnumber;
            info.trackingNumber = trackingnumber;

            if (response == "0")
            {

                info.success = false;
                info.errorCode = doc.GetValue("/TrackResponse/Response/Error/ErrorCode");
                info.errorDescription = doc.GetValue("/TrackResponse/Response/Error/ErrorDescription");

            }
            else
            {

                info.success = true;
                info.statusCode = doc.GetValue("/TrackResponse/Shipment/Package/Activity/Status/StatusCode/Code");
                info.status = doc.GetValue("/TrackResponse/Shipment/Package/Activity/Status/StatusType/Description");
                info.service = doc.GetValue("/TrackResponse/Shipment/Service/Description");

                string pickupDate = doc.GetValue("/TrackResponse/Shipment/PickupDate");
                                
                if (!String.IsNullOrEmpty(pickupDate))
                    info.pickedUp = DateTime.TryParse(pickupDate.Substring(4, 2) + "/" + pickupDate.Substring(6, 2) + "/" + pickupDate.Substring(0, 4), out info.pickedupdate);

                info.delivered = (info.status == "DELIVERED");

                if (info.delivered)
                {

                    string deliverDate = doc.GetValue("/TrackResponse/Shipment/Package/Activity/Date");
                    deliverDate = deliverDate.Substring(4, 2) + "/" + deliverDate.Substring(6, 2) + "/" + deliverDate.Substring(0, 4);

                    string deliverTime = doc.GetValue("/TrackResponse/Shipment/Package/Activity/Time");
                    deliverTime = deliverTime.Substring(0, 2) + ":" + deliverTime.Substring(2, 2);

                    DateTime.TryParse(deliverDate + " " + deliverTime, out info.deliveryDate);

                }

                info.shipFromAddress = doc.GetValue("/TrackResponse/Shipment/Shipper/Address/AddressLine1");
                info.shipFromCity = doc.GetValue("/TrackResponse/Shipment/Shipper/Address/StateProvinceCode");
                info.shipFromState = doc.GetValue("/TrackResponse/Shipment/Shipper/Address/StateProvinceCode");
                info.shipFromZip = doc.GetValue("/TrackResponse/Shipment/Shipper/Address/PostalCode");
                info.shipFromCountry = doc.GetValue("/TrackResponse/Shipment/Shipper/Address/CountryCode");

                info.shipToCity = doc.GetValue("/TrackResponse/Shipment/ShipTo/Address/City");
                info.shipToState = doc.GetValue("/TrackResponse/Shipment/ShipTo/Address/StateProvinceCode");
                info.shipToZip = doc.GetValue("/TrackResponse/Shipment/ShipTo/Address/PostalCode");
                info.shipToCountry = doc.GetValue("/TrackResponse/Shipment/ShipTo/Address/CountryCode");

            }

            return info;


        }

    }
}
