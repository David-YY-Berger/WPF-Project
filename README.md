# Imagine a World - Run by Drone Deliveries
>created by David Berger & Alexandr Sychev, at the Lev Institute (Jerusalem College of Technology)

### What we learned from this project:
We implemented a **3-Layered** model (Data, Logic, Presentation), with appropriate **interfaces**, **exceptions** and software design. 
The data itself is saved in **XML** docs (serialized by simple **LINQ** - see “stations”), and the data interface is protected by a simple **Singleton** (and **Factory**) pattern.    
The Logic layer runs **multiple Threads** simoultaneously (during ‘simulation’), protecting and locking **critical sections**, and computes battery life of drones and realistic longitude and latitude of customers and stations (using Haversine’s formula for distance).    
This was our first **GUI** project, without any framework, building everything from scratch - we learned about **EventHandlers**, **Data Binding**, **Progress updates**, etc.    
We learned to use **Git** (via GitHub)- coordinating commits and merges effectively, managing different branches and tags.  

![image](https://user-images.githubusercontent.com/91850832/151113497-51e38999-ec63-45c5-bc7f-823057499d80.png)




We developed a system (without a framework) to allow a company to track delivery of parcels from different customers, using drones.

There are four main objects held in The Data Library: 
- Drones, 
- Stations (to charge the drones), 
- Customers, 
- Parcels. 

### Directions to view the project easily:

- [ ] Click on “Default Login”
- [ ] Open the **Map Window**, and click “Start Simulator”. Watch the drones deliver the parcels. 

![image](https://user-images.githubusercontent.com/91850832/151113568-cfdd7a80-e7e7-478e-9602-8b1a66fa6a8d.png)


- [ ] Click "**Show TextBlock View**" \
_Blue boxes_ are customers - with the number of parcels waiting to be delivered \
_Green boxes_ are stations - with the number of drones charging there \
_Red boxes_ are drones - a 1 if they're carrying a parcel, and 0 if they're not carrying a parcel

![image](https://user-images.githubusercontent.com/91850832/151113656-d721aa1e-05b6-4902-bcab-2da433aca861.png)



- [ ] You can **view each object** in their window -> Open their list from the MainWindow and double click on an item. 

![image](https://user-images.githubusercontent.com/91850832/151113962-6608fe12-070f-496e-a9a3-312adaeace10.png)




- [ ]  To **manually deliver parcels** (instead of the Simulator), open an individual drone window. 
![image](https://user-images.githubusercontent.com/91850832/151115429-3ec90b42-6c59-4bdf-ba48-750a9b34c5db.png)


Enjoy!


### A bit about the back code:

**Data Layer (DL)**  - safely stores the objects in an XML file. Protects the data with a Singleton design pattern (Lazy Instantiation) and very simple Factory class.

**Business Logic Layer (BL)** - Accesses the Data Layer with a variable called “dataAccess”. 
This layer sifts thru the data to bring relevant objects, calculates some details...
This layer saves almost no data between calculations... the BO (Business Objects) are discarded after creation (except for BODrone) 

**Presentation Layer (PL)** - 
Accesses BL with “busiAccess”.
Refreshes the GUI regularly with updated data.


