using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    namespace BLApi
    {
        class EXCustInParcNotFoundException : Exception
        {
           public BO.BOCustomerInParcel creatEmptyCustInParc()
            {
                BO.BOCustomerInParcel ans = new BO.BOCustomerInParcel(-1,"");
                return ans;
            }
        }

        class EXParcInTransNotFoundException : Exception
        {
            public BO.BOParcelInTransfer creatEmptyParcInTrans()
            {
                BO.BOParcelInTransfer ans = new BO.BOParcelInTransfer();
                ans.Id = -1;
                return ans;
            }
        }

        public class EXNotFoundPrintException:Exception
        {
            public string ItemName { get; }

            public EXNotFoundPrintException(string name)
            {
                ItemName = name;
            }
            public override string ToString()
            {
                return ItemName + " not found";
            }
            public void printException()
            {
                Console.WriteLine(ItemName + " not found!/n");
            }

            public void printNotAvailableException()
            {
                Console.WriteLine(ItemName + " is not available!/n");
            }
        }


        public class EXDroneNotFound : EXNotFoundPrintException
        {
            public EXDroneNotFound() : base("Drone") { }
        }
        public class EXCustomerNotFound : EXNotFoundPrintException
        {
            public EXCustomerNotFound() : base("Customer") { }
        }
        public class EXSenderNotFound : EXCustomerNotFound { }
        public class EXReceiverNotFound : EXCustomerNotFound { }
        public class EXParcelNotFound : EXNotFoundPrintException
        {
            public EXParcelNotFound() : base("Parcel") { }
        }
        public class EXUsernameNotFound : EXNotFoundPrintException
        {
            public EXUsernameNotFound() : base("Username") { }
        }
        public class EXUserPasswordIncorrect : EXNotFoundPrintException
        {
            public EXUserPasswordIncorrect() : base("Incorrect Password") { }
            public override string ToString() { return ItemName; }
        }
        
    }
}
