using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    namespace BLApi
    {
        public partial class BusinessLayer : Ibl
        {

            //SIMULATOR
            [MethodImpl(MethodImplOptions.Synchronized)]
            public void BeginSimulatorForDrone(int droneId/*, Action<int> updatesToDo, Func<bool> stopSimulato*/)
            {
                new Thread(() => { wrapperFuncToBeginSimulator(droneId); }).Start();
            }
            public void wrapperFuncToBeginSimulator(int droneId) //allows us to use a thread with a parameter..
            {
                //calls the CTOR - creates new, unique simulator every time..
                //"sim" is saved as a field of the BL object
                SimulatorBL sim =  new SimulatorBL(this, droneId);
                foreach (var item in listSimulators)
                {
                    if (item.DroneId == sim.DroneId) //ensure that there is no simulator started already
                        return;
                }
                sim.StartSimulator();
                listSimulators.Add(sim);
            }
            [MethodImpl(MethodImplOptions.Synchronized)]
            public void StopSimulatorForDrone(int _droneId)
            {
                for (int i = 0; i < listSimulators.Count; i++)
                {
                    if(listSimulators[i].DroneId == _droneId)
                    {
                        listSimulators[i].StopSimulator();
                        listSimulators.RemoveAt(i);
                        return;
                    }
                }
            }
            //end of class..
        }
    }
}