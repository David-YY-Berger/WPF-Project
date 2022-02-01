using System;

namespace DalXml
{
    namespace DataObject
    {

        public struct Parcel
        {
            public Parcel(int _senderId,int _targetId,DalXml.DataObject.WeightCategories? _weight,
                          DalXml.DataObject.Priorities? _priority) 
            {
                Id = DalXml.DataSource.thisConfig.parcelSerialNumber++; 
                SenderId = _senderId;
                ReceiverId = _targetId;
                Weight = _weight;
                Priority =_priority;
                TimeCreated = DateTime.Now; //when we receive request for the parcel
                DroneId = -1;             //null...
                TimeAssigned = null;
                TimePickedUp = null;     //null...
                TimeDelivered = null; //null...
                Exists = true;
            }

            public int Id { get; set; }
            public int SenderId { get; set; }
            public int ReceiverId { get; set; }
            public DalXml.DataObject.WeightCategories? Weight { get; set; }
            public DalXml.DataObject.Priorities? Priority { get; set; }
            public int? DroneId { get; set; }
            
            public DateTime? TimeCreated { get; set; } //when we receive request for the parcel
            public DateTime ? TimeAssigned { get; set; }
            public DateTime? TimePickedUp { get; set; } //from Sender
            public DateTime? TimeDelivered { get; set; } //at Receipient 
            public bool Exists { get; set; }

            public override string ToString()
            {
                string res = "";
                res = "Parcel: " + Id.ToString() + "\n" ;
                res += "SenderId: " + SenderId + "\n" +
                    "ReceiverId: " + ReceiverId + "\n" +
                    "Weight: " + Weight + "\n" +
                    "Priority: " + Priority + "\n" +
                    "Requested: " + TimeCreated + "\n" +
                    "DroneId: " + DroneId + "\n" +
                    "Scheduled: " + TimeAssigned + "\n" +
                    "Pickup: " + TimePickedUp + "\n" +
                    "Delivered: " + TimeDelivered + "\n";
                return res;
            }


            //public void print()
            //{
            //    Console.WriteLine("Parcel: " + Id + "\n" +
            //        "SenderId: " + SenderId + "\n" +
            //        "ReceiverId: " + ReceiverId + "\n" +
            //        "Weight: " + Weight + "\n" +
            //        "Priority: " + Priority + "\n" +
            //        "Requested: " + Requested + "\n" +
            //        "DroneId: " + DroneId + "\n" +
            //        "Scheduled: " + Scheduled + "\n" +
            //        "Pickup: " + Pickup + "\n" +
            //        "Delivered: " + Delivered + "\n"
            //        );
            //}



        }


    }

}
