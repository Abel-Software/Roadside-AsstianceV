//Version 0.3
using System;
using System.Collections.Generic;
using System.Drawing;
using Size = System.Drawing.Size;
using Point = System.Drawing.Point;
using System.Windows.Forms;
using System.Linq;
using System.Text;
using GTA;
using GTA.Native;
using GTA.Math;
using GTA.NaturalMotion;
using NativeUI;
using NativeUI.PauseMenu;

public class RSA : Script
{
    private Ped playerPed = Game.Player.Character;
    private Player player = Game.Player;
    private MenuPool _menuPool;

    //INI File
    ScriptSettings config;
    Keys OnCall;
    Keys OpenMenu;
    Keys ForceCall;
    Keys StreetName;
    Keys Radio;
    string unit;
    string name;
    string RSAVehicleModel;

    public void RoadSideServices(UIMenu menu)
    {
        var SubRoadSideServices = _menuPool.AddSubMenu(menu, "Roadside Services");
        for (int i = 0; i < 1; i++) ;

        //Mechanic (DONE)
        var mechanic = new UIMenuItem("Request Mechanic - $100", "Request for a mechanic to come to your current location to evaluate and repair your vehicle at a price of $100.");
        SubRoadSideServices.AddItem(mechanic);
        SubRoadSideServices.OnItemSelect += (sender, item, index) =>
        {
            if (item == mechanic)
            {
                Game.Player.Money -= 100;
                UI.Notify("~y~A mechanic is on the way. Please wait for him to arrive!");

                //Determine Player Location
                Ped player = Game.Player.Character;
                GTA.Math.Vector3 playerlocation = Game.Player.Character.Position.Around(5f);
                GTA.Math.Vector3 spawnlocation = World.GetNextPositionOnStreet(Game.Player.Character.Position.Around(15f));

                //Create Model
                string mech = "S_M_Y_XMECH_01";
                Ped mechped = GTA.World.CreatePed(mech, spawnlocation);

                //Task
                Vehicle towtruck = World.CreateVehicle(RSAVehicleModel, spawnlocation);
                towtruck.SirenActive = true;
                mechped.SetIntoVehicle(towtruck, VehicleSeat.Driver);
                Function.Call(Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE, mechped, towtruck, playerlocation.X, playerlocation.Y, playerlocation.Z, 30f, 262144, 1f);
                Wait(7000);

                //Mechanic Talking
                List<string> mechanicspeaks = new List<string>();
                mechanicspeaks.Add("~y~Looks like a simple repair!");
                mechanicspeaks.Add("~y~Your car is pretty beat up, might need to call a tow truck!");
                mechanicspeaks.Add("~y~Did you let you kid drive your car again?!");
                mechanicspeaks.Add("~y~I hope you have good insurance!");
                Random rnd2 = new Random();
                UI.Notify(mechanicspeaks[rnd2.Next(0, 3)]);
                
                //Go To Car
                mechped.Task.LeaveVehicle();
                mechped.Task.GoTo(Game.Player.Character);
                Wait(12000);
                Game.Player.Character.CurrentVehicle.Repair();
                Game.Player.Character.LastVehicle.Repair();
                Wait(7000);
                mechped.Task.GoTo(towtruck);
                mechped.SetIntoVehicle(towtruck, VehicleSeat.Driver);
                Function.Call(Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE, mechped, towtruck, -221.749908f, -1158.249756f, 23.040998f, 30f, 262144, 1f);
                Wait(25000);
                mechped.Delete();
                towtruck.Delete();
            }
        };

        //Tow Vehicle (DONE)
        var towvehicle = new UIMenuItem("Tow Vehicle - $300", "Request for a tow truck driver to come to your current location and tow your vehicle to the shop at a price of $300.");
        SubRoadSideServices.AddItem(towvehicle);
        SubRoadSideServices.OnItemSelect += (sender, item, index) =>
        {
            if (item == towvehicle)
            {
                Game.Player.Money -= 300;
                Game.Player.Character.CurrentVehicle.IsPersistent = true;

                //SET
                GTA.Math.Vector3 spawnlocation = World.GetNextPositionOnStreet(Game.Player.Character.Position.Around(15f));
                string mech = "S_M_Y_XMECH_01";
                Ped mechped = GTA.World.CreatePed(mech, spawnlocation);
                Ped player = Game.Player.Character;
                GTA.Math.Vector3 playerlocation = Game.Player.Character.Position.Around(5f);

                //Create List and showing subtitle
                List<string> towspeaks = new List<string>();
                towspeaks.Add("~y~Did someone call for a tow truck?");
                towspeaks.Add("~y~Looks like you could use some help!");
                towspeaks.Add("~y~Looks like you bought a Ford vehicle.");
                towspeaks.Add("~y~How long have you been waiting on me to get here?");
                towspeaks.Add("~y~Sorry about the wait, the drive through line took forever!");
                towspeaks.Add("~y~Sorry I took 2 hours, had to finish lunch.");
                Random rnd = new Random();
                UI.ShowSubtitle(towspeaks[rnd.Next(0, 5)]);

                //TOW
                Vehicle car = Game.Player.Character.CurrentVehicle;
                Vehicle towtruck = World.CreateVehicle("TOWTRUCK", spawnlocation);
                towtruck.IsPersistent = true;
                mechped.SetIntoVehicle(towtruck, VehicleSeat.Driver);
                Function.Call(Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE, mechped, towtruck, playerlocation.X, playerlocation.Y, playerlocation.Z, 30f, 786603, 1f);
                Wait(3000);
                Function.Call(Hash.ATTACH_VEHICLE_TO_TOW_TRUCK, towtruck, car);
                towtruck.LockStatus = VehicleLockStatus.LockedForPlayer;
                towtruck.LockStatus = VehicleLockStatus.StickPlayerInside;

                //SET PLAYER IN TRUCK & TELEPORTS
                Game.Player.CanControlCharacter = false;
                Game.Player.Character.SetIntoVehicle(towtruck, VehicleSeat.RightFront);
                Game.FadeScreenOut(5000);
                Wait(7000);
                towtruck.Position = new GTA.Math.Vector3(-221.749908f, -1158.249756f, 23.040998f);
                car.Position = new GTA.Math.Vector3(-221.749908f * -1, -1158.249756f * -1, 23.040998f * -1);
                Function.Call(Hash.ATTACH_VEHICLE_TO_TOW_TRUCK, towtruck, car);
                mechped.Position = new GTA.Math.Vector3(-221.749908f, -1158.249756f, 23.040998f);
                Game.Player.Character.Position = new GTA.Math.Vector3(-221.749908f, -1158.249756f, 23.040998f);
                Game.Player.Character.SetIntoVehicle(towtruck, VehicleSeat.RightFront);
                Game.Player.Character.LastVehicle.Repair();
                Wait(3000);
                Game.FadeScreenIn(5000);
                Wait(3000);
                towtruck.DetachTowedVehicle();
                Wait(3000);
                car.Position = new GTA.Math.Vector3(-236.07f, -1173.09f, 22.90f);
                Wait(5000);
                towtruck.Delete();
                mechped.Delete();
                Game.Player.CanControlCharacter = true;

                //FUNNY TEXT
                List<string> towspeaks2 = new List<string>();
                towspeaks2.Add("~y~Your vehicle has been towed!");
                towspeaks2.Add("~y~Thank you for your service with Roadside Assistance!");
                towspeaks2.Add("~y~Next time be sure to buy a Dodge!");
                towspeaks2.Add("~y~Next time be sure to buy a Chevy!");
                towspeaks2.Add("~y~Thank you for using Roadside Assistance! Next time I recommend a better tow driver!");
                towspeaks2.Add("~y~Roadside Assistance doesn't covered damage done during the tow process! Have a nice day!");
                Random rnd5 = new Random();
                UI.ShowSubtitle(towspeaks2[rnd5.Next(0, 5)]);
            }
        };

        //Report Accident (DONE)
        var accidentreport = new UIMenuItem("Report Accident - $15", "Place a call to Roadside Assistance Services to report a motor vehicle accident. There will be a $15 charge for the call.");
        SubRoadSideServices.AddItem(accidentreport);
        SubRoadSideServices.OnItemSelect += (sender, item, index) =>
        {
            if (item == accidentreport)
            {
                //Take Money
                Game.Player.Money -= 15;

                //Dialog
                UI.Notify("~y~A call is being placed to a Roadside Assistance Agent, please wait.");
                Wait(5000);
                UI.ShowSubtitle("~y~Thank you for calling Roadside Assistance!");
                Wait(5000);
                UI.ShowSubtitle("~y~I understand that you've been in an accident that you would like to report.");
                Wait(5000);
                UI.ShowSubtitle("~y~We have dispatched emergency services to your location as well as a Roadside Assistance Agent to evaluate your vehicle and the damages!");
                Wait(5000);  

                //Set
                GTA.Math.Vector3 spawnlocation = World.GetNextPositionOnStreet(Game.Player.Character.Position.Around(30f));
                string cop = "S_M_Y_COP_01";
                string mech = "S_M_Y_XMECH_01";
                Ped copped = GTA.World.CreatePed(cop, spawnlocation);
                Ped player = Game.Player.Character;
                GTA.Math.Vector3 playerlocation = Game.Player.Character.Position.Around(5f);
                GTA.Math.Vector3 playercarlocation = Game.Player.Character.LastVehicle.Position.Around(5f);
                Model clipboard = "P_CS_Clipboard";
                Ped mechped = GTA.World.CreatePed(mech, spawnlocation);
                
                //Police Response
                Vehicle policecar = World.CreateVehicle("POLICE", spawnlocation);
                Vehicle mechcar = World.CreateVehicle("TOWTRUCK", spawnlocation);
                policecar.SirenActive = true;
                copped.SetIntoVehicle(policecar, VehicleSeat.Driver);
                mechped.SetIntoVehicle(mechcar, VehicleSeat.Driver);
                Function.Call(Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE, copped, policecar, playerlocation.X, playerlocation.Y, playerlocation.Z, 30f, 262144, 1f);
                Function.Call(Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE, mechped, mechcar, playercarlocation.X, playercarlocation.Y, playercarlocation.Z, 30f, 262144, 1f);
                Wait(9000);
                copped.Task.LeaveVehicle();
                copped.Task.GoTo(Game.Player.Character.Position);
                mechped.Task.LeaveVehicle();
                mechped.Task.GoTo(Game.Player.Character.LastVehicle);
                Wait(6000);
                UI.ShowSubtitle("~w~Cop: ~b~Hey there! You must be " + name + "!");
                Wait(5000);
                UI.Notify("~y~The officer is evaluating the accident!");
                UI.Notify("~y~The mechanic is evaluating the vehicle!");
                copped.Task.PlayAnimation("misslsdhsclipboard@base", "base", 0, 25000, AnimationFlags.Loop);
                mechped.Task.PlayAnimation("misslsdhsclipboard@base", "base", 0, 25000, AnimationFlags.Loop);
                Wait(25000);
                copped.Task.ClearAnimation("misslsdhsclipboard@base", "base");
                mechped.Task.ClearAnimation("misslsdhsclipboard@base", "base");
                Wait(2000);
                UI.ShowSubtitle("~w~Cop: ~b~Alright " + name + ", your report has been claimed with us and your insurance company!");
                Wait(3000);
                copped.Task.GoTo(policecar.Position);
                mechped.Task.GoTo(mechcar.Position);
                Wait(6000);
                copped.SetIntoVehicle(policecar, VehicleSeat.Driver);
                mechped.SetIntoVehicle(mechcar, VehicleSeat.Driver);
                Function.Call(Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE, copped, policecar, -221.749908f, -1158.249756f, 23.040998f, 30f, 262144, 1f);
                Function.Call(Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE, mechped, mechcar, -221.749908f, -1158.249756f, 23.040998f, 30f, 262144, 1f);
                Wait(15000);
                copped.Delete();
                policecar.Delete();
                mechped.Delete();
                mechcar.Delete();
            }
        };
    } //WIP

    public void RentalServices(UIMenu menu)
    {
        var SubRentalServices = _menuPool.AddSubMenu(menu, "Rental Services");
        for (int i = 0; i < 1; i++) ;

        //COUPES HEADER
        var coupeheader = new UIMenuItem("~r~Coupes - ~g~$1,500", "View the following coupes available for rent");
        SubRentalServices.AddItem(coupeheader);
        SubRentalServices.OnItemSelect += (sender, item, index) =>
        {
            if (item == coupeheader)
            {

            }
        };

        //Dilettante
        var Dilettante = new UIMenuItem("Rent Dilettante - $1,500", "Rent the gas saving Dilettante from our rental agency for only $1,500!");
        SubRentalServices.AddItem(Dilettante);
        SubRentalServices.OnItemSelect += (sender, item, index) =>
        {
            if (item == Dilettante)
            {
                //MONEY TAKEN
                Game.Player.Money -= 1500;
                UI.Notify("~o~Processing request...");

                //SCREEN FADE AND TELEPORT
                Game.FadeScreenOut(5000);
                Wait(7000);
                Game.Player.Character.Position = new GTA.Math.Vector3(-55.54f, -1111.43f, 26.21f);
                GTA.Math.Vector3 spawnlocation = new GTA.Math.Vector3(-55.54f, -1111.43f, 26.21f);
                Vehicle car = World.CreateVehicle("DILETTANTE", spawnlocation);
                Game.Player.Character.SetIntoVehicle(car, VehicleSeat.Driver);
                Wait(3000);
                Game.FadeScreenIn(5000);
                Wait(2000);
                UI.Notify("~o~Processing complete!");
                UI.Notify("~g~Enjoy your vehicle!");
                Function.Call(Hash.PLAY_MISSION_COMPLETE_AUDIO);
            }
        };

        //Jackal
        var Jackal = new UIMenuItem("Rent Jackal - $1,500", "Rent the Jackal from our rental agency for only $1,500!");
        SubRentalServices.AddItem(Jackal);
        SubRentalServices.OnItemSelect += (sender, item, index) =>
        {
            if (item == Jackal)
            {
                //MONEY TAKEN
                Game.Player.Money -= 1500;
                UI.Notify("~o~Processing request...");

                //SCREEN FADE AND TELEPORT
                Game.FadeScreenOut(5000);
                Wait(7000);
                Game.Player.Character.Position = new GTA.Math.Vector3(-55.54f, -1111.43f, 26.21f);
                GTA.Math.Vector3 spawnlocation = new GTA.Math.Vector3(-55.54f, -1111.43f, 26.21f);
                Vehicle car = World.CreateVehicle("JACKAL", spawnlocation);
                Game.Player.Character.SetIntoVehicle(car, VehicleSeat.Driver);
                Wait(3000);
                Game.FadeScreenIn(5000);
                Wait(2000);
                UI.Notify("~o~Processing complete!");
                UI.Notify("~g~Enjoy your vehicle!");
                Function.Call(Hash.PLAY_MISSION_COMPLETE_AUDIO); 
            }
        };

        //Zion
        var Zion = new UIMenuItem("Rent Zion - $1,500", "Rent the Zion from our rental agency for only $1,500!");
        SubRentalServices.AddItem(Zion);
        SubRentalServices.OnItemSelect += (sender, item, index) =>
        {
            if (item == Zion)
            {
                //MONEY TAKEN
                Game.Player.Money -= 1500;
                UI.Notify("~o~Processing request...");

                //SCREEN FADE AND TELEPORT
                Game.FadeScreenOut(5000);
                Wait(7000);
                Game.Player.Character.Position = new GTA.Math.Vector3(-55.54f, -1111.43f, 26.21f);
                GTA.Math.Vector3 spawnlocation = new GTA.Math.Vector3(-55.54f, -1111.43f, 26.21f);
                Vehicle car = World.CreateVehicle("ZION", spawnlocation);
                Game.Player.Character.SetIntoVehicle(car, VehicleSeat.Driver);
                Wait(3000);
                Game.FadeScreenIn(5000);
                Wait(2000);
                UI.Notify("~o~Processing complete!");
                UI.Notify("~g~Enjoy your vehicle!");
                Function.Call(Hash.PLAY_MISSION_COMPLETE_AUDIO); 
            }
        };

        //MUSCLE HEADER
        var muscleheader = new UIMenuItem("~r~Muscle Cars - ~g~$2,500", "View the following muscle cars available for rent");
        SubRentalServices.AddItem(muscleheader);
        SubRentalServices.OnItemSelect += (sender, item, index) =>
        {
            if (item == muscleheader)
            {

            }
        };

        //Gauntlet
        var Gauntlet = new UIMenuItem("Rent Gauntlet - $2,500", "Rent the Gauntlet from our rental agency for only $2,500!");
        SubRentalServices.AddItem(Gauntlet);
        SubRentalServices.OnItemSelect += (sender, item, index) =>
        {
            if (item == Gauntlet)
            {
                //MONEY TAKEN
                Game.Player.Money -= 2500;
                UI.Notify("~o~Processing request...");

                //SCREEN FADE AND TELEPORT
                Game.FadeScreenOut(5000);
                Wait(7000);
                Game.Player.Character.Position = new GTA.Math.Vector3(-55.54f, -1111.43f, 26.21f);
                GTA.Math.Vector3 spawnlocation = new GTA.Math.Vector3(-55.54f, -1111.43f, 26.21f);
                Vehicle car = World.CreateVehicle("GAUNTLET", spawnlocation);
                Game.Player.Character.SetIntoVehicle(car, VehicleSeat.Driver);
                Wait(3000);
                Game.FadeScreenIn(5000);
                Wait(2000);
                UI.Notify("~o~Processing complete!");
                UI.Notify("~g~Enjoy your vehicle!");
                Function.Call(Hash.PLAY_MISSION_COMPLETE_AUDIO); 
            }
        };

        //Ruiner
        var Ruiner = new UIMenuItem("Rent Ruiner - $2,500", "Rent the Ruiner from our rental agency for only $2,500!");
        SubRentalServices.AddItem(Ruiner);
        SubRentalServices.OnItemSelect += (sender, item, index) =>
        {
            if (item == Ruiner)
            {
                //MONEY TAKEN
                Game.Player.Money -= 2500;
                UI.Notify("~o~Processing request...");

                //SCREEN FADE AND TELEPORT
                Game.FadeScreenOut(5000);
                Wait(7000);
                Game.Player.Character.Position = new GTA.Math.Vector3(-55.54f, -1111.43f, 26.21f);
                GTA.Math.Vector3 spawnlocation = new GTA.Math.Vector3(-55.54f, -1111.43f, 26.21f);
                Vehicle car = World.CreateVehicle("RUINER", spawnlocation);
                Game.Player.Character.SetIntoVehicle(car, VehicleSeat.Driver);
                Wait(3000);
                Game.FadeScreenIn(5000);
                Wait(2000);
                UI.Notify("~o~Processing complete!");
                UI.Notify("~g~Enjoy your vehicle!");
                Function.Call(Hash.PLAY_MISSION_COMPLETE_AUDIO); 
            }
        };

        //SEDANS HEADER
        var sedanheader = new UIMenuItem("~r~Muscle Cars - ~g~$2,250", "View the following sedan cars available for rent");
        SubRentalServices.AddItem(sedanheader);
        SubRentalServices.OnItemSelect += (sender, item, index) =>
        {
            if (item == sedanheader)
            {

            }
        };

        //PRIMO
        var PRIMO = new UIMenuItem("Rent Primo - $2,250", "Rent the Primo from our rental agency for only $2,250!");
        SubRentalServices.AddItem(PRIMO);
        SubRentalServices.OnItemSelect += (sender, item, index) =>
        {
            if (item == PRIMO)
            {
                //MONEY TAKEN
                Game.Player.Money -= 2250;
                UI.Notify("~o~Processing request...");

                //SCREEN FADE AND TELEPORT
                Game.FadeScreenOut(5000);
                Wait(7000);
                Game.Player.Character.Position = new GTA.Math.Vector3(-55.54f, -1111.43f, 26.21f);
                GTA.Math.Vector3 spawnlocation = new GTA.Math.Vector3(-55.54f, -1111.43f, 26.21f);
                Vehicle car = World.CreateVehicle("PRIMO", spawnlocation);
                Game.Player.Character.SetIntoVehicle(car, VehicleSeat.Driver);
                Wait(3000);
                Game.FadeScreenIn(5000);
                Wait(2000);
                UI.Notify("~o~Processing complete!");
                UI.Notify("~g~Enjoy your vehicle!");
                Function.Call(Hash.PLAY_MISSION_COMPLETE_AUDIO); 
            }
        };

        //Fugitive
        var Fugitive = new UIMenuItem("Rent Fugitive - $2,250", "Rent the Fugitive from our rental agency for only $2,250!");
        SubRentalServices.AddItem(Fugitive);
        SubRentalServices.OnItemSelect += (sender, item, index) =>
        {
            if (item == Fugitive)
            {
                //MONEY TAKEN
                Game.Player.Money -= 2250;
                UI.Notify("~o~Processing request...");

                //SCREEN FADE AND TELEPORT
                Game.FadeScreenOut(5000);
                Wait(7000);
                Game.Player.Character.Position = new GTA.Math.Vector3(-55.54f, -1111.43f, 26.21f);
                GTA.Math.Vector3 spawnlocation = new GTA.Math.Vector3(-55.54f, -1111.43f, 26.21f);
                Vehicle car = World.CreateVehicle("FUGITIVE", spawnlocation);
                Game.Player.Character.SetIntoVehicle(car, VehicleSeat.Driver);
                Wait(3000);
                Game.FadeScreenIn(5000);
                Wait(2000);
                UI.Notify("~o~Processing complete!");
                UI.Notify("~g~Enjoy your vehicle!");
                Function.Call(Hash.PLAY_MISSION_COMPLETE_AUDIO);
            }
        };

        //Asea
        var Asea = new UIMenuItem("Rent Asea - $2,250", "Rent the Asea from our rental agency for only $2,250!");
        SubRentalServices.AddItem(Asea);
        SubRentalServices.OnItemSelect += (sender, item, index) =>
        {
            if (item == Asea)
            {
                //MONEY TAKEN
                Game.Player.Money -= 2250;
                UI.Notify("~o~Processing request...");

                //SCREEN FADE AND TELEPORT
                Game.FadeScreenOut(5000);
                Wait(7000);
                Game.Player.Character.Position = new GTA.Math.Vector3(-55.54f, -1111.43f, 26.21f);
                GTA.Math.Vector3 spawnlocation = new GTA.Math.Vector3(-55.54f, -1111.43f, 26.21f);
                Vehicle car = World.CreateVehicle("ASEA", spawnlocation);
                Game.Player.Character.SetIntoVehicle(car, VehicleSeat.Driver);
                Wait(3000);
                Game.FadeScreenIn(5000);
                Wait(2000);
                UI.Notify("~o~Processing complete!");
                UI.Notify("~g~Enjoy your vehicle!");
                Function.Call(Hash.PLAY_MISSION_COMPLETE_AUDIO);
            }
        };

        //Tailgater
        var Tailgater = new UIMenuItem("Rent Tailgater - $2,250", "Rent the Tailgater from our rental agency for only $2,250!");
        SubRentalServices.AddItem(Tailgater);
        SubRentalServices.OnItemSelect += (sender, item, index) =>
        {
            if (item == Tailgater)
            {
                //MONEY TAKEN
                Game.Player.Money -= 2250;
                UI.Notify("~o~Processing request...");

                //SCREEN FADE AND TELEPORT
                Game.FadeScreenOut(5000);
                Wait(7000);
                Game.Player.Character.Position = new GTA.Math.Vector3(-55.54f, -1111.43f, 26.21f);
                GTA.Math.Vector3 spawnlocation = new GTA.Math.Vector3(-55.54f, -1111.43f, 26.21f);
                Vehicle car = World.CreateVehicle("TAILGATER", spawnlocation);
                Game.Player.Character.SetIntoVehicle(car, VehicleSeat.Driver);
                Wait(3000);
                Game.FadeScreenIn(5000);
                Wait(2000);
                UI.Notify("~o~Processing complete!");
                UI.Notify("~g~Enjoy your vehicle!");
                Function.Call(Hash.PLAY_MISSION_COMPLETE_AUDIO);
            }
        };

        //Stanier
        var Stanier = new UIMenuItem("Rent Stanier - $2,250", "Rent the Stanier from our rental agency for only $2,250!");
        SubRentalServices.AddItem(Stanier);
        SubRentalServices.OnItemSelect += (sender, item, index) =>
        {
            if (item == Stanier)
            {
                //MONEY TAKEN
                Game.Player.Money -= 2250;
                UI.Notify("~o~Processing request...");

                //SCREEN FADE AND TELEPORT
                Game.FadeScreenOut(5000);
                Wait(7000);
                Game.Player.Character.Position = new GTA.Math.Vector3(-55.54f, -1111.43f, 26.21f);
                GTA.Math.Vector3 spawnlocation = new GTA.Math.Vector3(-55.54f, -1111.43f, 26.21f);
                Vehicle car = World.CreateVehicle("STANIER", spawnlocation);
                Game.Player.Character.SetIntoVehicle(car, VehicleSeat.Driver);
                Wait(3000);
                Game.FadeScreenIn(5000);
                Wait(2000);
                UI.Notify("~o~Processing complete!");
                UI.Notify("~g~Enjoy your vehicle!");
                Function.Call(Hash.PLAY_MISSION_COMPLETE_AUDIO);
            }
        };

        //Trucks HEADER
        var trucksheader = new UIMenuItem("~r~Trucks - ~g~$5,000", "View the following trucks available for rent");
        SubRentalServices.AddItem(trucksheader);
        SubRentalServices.OnItemSelect += (sender, item, index) =>
        {
            if (item == trucksheader)
            {

            }
        };

        //Bison
        var Bison = new UIMenuItem("Rent Bison - $5,000", "Rent the Bison from our rental agency for only $5,000!");
        SubRentalServices.AddItem(Bison);
        SubRentalServices.OnItemSelect += (sender, item, index) =>
        {
            if (item == Bison)
            {
                //MONEY TAKEN
                Game.Player.Money -= 5000;
                UI.Notify("~o~Processing request...");

                //SCREEN FADE AND TELEPORT
                Game.FadeScreenOut(5000);
                Wait(7000);
                Game.Player.Character.Position = new GTA.Math.Vector3(-55.54f, -1111.43f, 26.21f);
                GTA.Math.Vector3 spawnlocation = new GTA.Math.Vector3(-55.54f, -1111.43f, 26.21f);
                Vehicle car = World.CreateVehicle("BISON", spawnlocation);
                Game.Player.Character.SetIntoVehicle(car, VehicleSeat.Driver);
                Wait(3000);
                Game.FadeScreenIn(5000);
                Wait(2000);
                UI.Notify("~o~Processing complete!");
                UI.Notify("~g~Enjoy your vehicle!");
                Function.Call(Hash.PLAY_MISSION_COMPLETE_AUDIO);
            }
        };

        //Sadler
        var Sadler = new UIMenuItem("Rent Sadler - $5,000", "Rent the Sadler from our rental agency for only $5,000!");
        SubRentalServices.AddItem(Sadler);
        SubRentalServices.OnItemSelect += (sender, item, index) =>
        {
            if (item == Sadler)
            {
                //MONEY TAKEN
                Game.Player.Money -= 5000;
                UI.Notify("~o~Processing request...");

                //SCREEN FADE AND TELEPORT
                Game.FadeScreenOut(5000);
                Wait(7000);
                Game.Player.Character.Position = new GTA.Math.Vector3(-55.54f, -1111.43f, 26.21f);
                GTA.Math.Vector3 spawnlocation = new GTA.Math.Vector3(-55.54f, -1111.43f, 26.21f);
                Vehicle car = World.CreateVehicle("SADLER", spawnlocation);
                Game.Player.Character.SetIntoVehicle(car, VehicleSeat.Driver);
                Wait(3000);
                Game.FadeScreenIn(5000);
                Wait(2000);
                UI.Notify("~o~Processing complete!");
                UI.Notify("~g~Enjoy your vehicle!");
                Function.Call(Hash.PLAY_MISSION_COMPLETE_AUDIO);
            }
        };

        //Sandking
        var Sandking = new UIMenuItem("Rent Sandking - $5,000", "Rent the Sandking from our rental agency for only $5,000!");
        SubRentalServices.AddItem(Sandking);
        SubRentalServices.OnItemSelect += (sender, item, index) =>
        {
            if (item == Sandking)
            {
                //MONEY TAKEN
                Game.Player.Money -= 5000;
                UI.Notify("~o~Processing request...");

                //SCREEN FADE AND TELEPORT
                Game.FadeScreenOut(5000);
                Wait(7000);
                Game.Player.Character.Position = new GTA.Math.Vector3(-55.54f, -1111.43f, 26.21f);
                GTA.Math.Vector3 spawnlocation = new GTA.Math.Vector3(-55.54f, -1111.43f, 26.21f);
                Vehicle car = World.CreateVehicle("SANDKING", spawnlocation);
                Game.Player.Character.SetIntoVehicle(car, VehicleSeat.Driver);
                Wait(3000);
                Game.FadeScreenIn(5000);
                Wait(2000);
                UI.Notify("~o~Processing complete!");
                UI.Notify("~g~Enjoy your vehicle!");
                Function.Call(Hash.PLAY_MISSION_COMPLETE_AUDIO);
            }
        };

        //SUV HEADER
        var suvheader = new UIMenuItem("~r~SUV's - ~g~$3,500", "View the following SUV's available for rent");
        SubRentalServices.AddItem(suvheader);
        SubRentalServices.OnItemSelect += (sender, item, index) =>
        {
            if (item == suvheader)
            {

            }
        };

        //Cavalcade
        var Cavalcade = new UIMenuItem("Rent Cavalcade - $3,500", "Rent the Cavalcade from our rental agency for only $3,500!");
        SubRentalServices.AddItem(Cavalcade);
        SubRentalServices.OnItemSelect += (sender, item, index) =>
        {
            if (item == Cavalcade)
            {
                //MONEY TAKEN
                Game.Player.Money -= 3500;
                UI.Notify("~o~Processing request...");

                //SCREEN FADE AND TELEPORT
                Game.FadeScreenOut(5000);
                Wait(7000);
                Game.Player.Character.Position = new GTA.Math.Vector3(-55.54f, -1111.43f, 26.21f);
                GTA.Math.Vector3 spawnlocation = new GTA.Math.Vector3(-55.54f, -1111.43f, 26.21f);
                Vehicle car = World.CreateVehicle("CAVALCADE", spawnlocation);
                Game.Player.Character.SetIntoVehicle(car, VehicleSeat.Driver);
                Wait(3000);
                Game.FadeScreenIn(5000);
                Wait(2000);
                UI.Notify("~o~Processing complete!");
                UI.Notify("~g~Enjoy your vehicle!");
                Function.Call(Hash.PLAY_MISSION_COMPLETE_AUDIO);
            }
        };

        //Gresley
        var Gresley = new UIMenuItem("Rent Gresley - $3,500", "Rent the Gresley from our rental agency for only $3,500!");
        SubRentalServices.AddItem(Gresley);
        SubRentalServices.OnItemSelect += (sender, item, index) =>
        {
            if (item == Gresley)
            {
                //MONEY TAKEN
                Game.Player.Money -= 3500;
                UI.Notify("~o~Processing request...");

                //SCREEN FADE AND TELEPORT
                Game.FadeScreenOut(5000);
                Wait(7000);
                Game.Player.Character.Position = new GTA.Math.Vector3(-55.54f, -1111.43f, 26.21f);
                GTA.Math.Vector3 spawnlocation = new GTA.Math.Vector3(-55.54f, -1111.43f, 26.21f);
                Vehicle car = World.CreateVehicle("GRESLEY", spawnlocation);
                Game.Player.Character.SetIntoVehicle(car, VehicleSeat.Driver);
                Wait(3000);
                Game.FadeScreenIn(5000);
                Wait(2000);
                UI.Notify("~o~Processing complete!");
                UI.Notify("~g~Enjoy your vehicle!");
                Function.Call(Hash.PLAY_MISSION_COMPLETE_AUDIO);
            }
        };

        //Granger
        var Granger = new UIMenuItem("Rent Granger - $3,500", "Rent the Granger from our rental agency for only $3,500!");
        SubRentalServices.AddItem(Granger);
        SubRentalServices.OnItemSelect += (sender, item, index) =>
        {
            if (item == Granger)
            {
                //MONEY TAKEN
                Game.Player.Money -= 3500;
                UI.Notify("~o~Processing request...");

                //SCREEN FADE AND TELEPORT
                Game.FadeScreenOut(5000);
                Wait(7000);
                Game.Player.Character.Position = new GTA.Math.Vector3(-55.54f, -1111.43f, 26.21f);
                GTA.Math.Vector3 spawnlocation = new GTA.Math.Vector3(-55.54f, -1111.43f, 26.21f);
                Vehicle car = World.CreateVehicle("GRANGER", spawnlocation);
                Game.Player.Character.SetIntoVehicle(car, VehicleSeat.Driver);
                Wait(3000);
                Game.FadeScreenIn(5000);
                Wait(2000);
                UI.Notify("~o~Processing complete!");
                UI.Notify("~g~Enjoy your vehicle!");
                Function.Call(Hash.PLAY_MISSION_COMPLETE_AUDIO);
            }
        };

        //Baller
        var Baller = new UIMenuItem("Rent Baller - $3,500", "Rent the Baller from our rental agency for only $3,500!");
        SubRentalServices.AddItem(Baller);
        SubRentalServices.OnItemSelect += (sender, item, index) =>
        {
            if (item == Baller)
            {
                //MONEY TAKEN
                Game.Player.Money -= 3500;
                UI.Notify("~o~Processing request...");

                //SCREEN FADE AND TELEPORT
                Game.FadeScreenOut(5000);
                Wait(7000);
                Game.Player.Character.Position = new GTA.Math.Vector3(-55.54f, -1111.43f, 26.21f);
                GTA.Math.Vector3 spawnlocation = new GTA.Math.Vector3(-55.54f, -1111.43f, 26.21f);
                Vehicle car = World.CreateVehicle("BALLER", spawnlocation);
                Game.Player.Character.SetIntoVehicle(car, VehicleSeat.Driver);
                Wait(3000);
                Game.FadeScreenIn(5000);
                Wait(2000);
                UI.Notify("~o~Processing complete!");
                UI.Notify("~g~Enjoy your vehicle!");
                Function.Call(Hash.PLAY_MISSION_COMPLETE_AUDIO);
            }
        };

        //Radius
        var Radius = new UIMenuItem("Rent Radius - $3,500", "Rent the Radius from our rental agency for only $3,500!");
        SubRentalServices.AddItem(Radius);
        SubRentalServices.OnItemSelect += (sender, item, index) =>
        {
            if (item == Radius)
            {
                //MONEY TAKEN
                Game.Player.Money -= 3500;
                UI.Notify("~o~Processing request...");

                //SCREEN FADE AND TELEPORT
                Game.FadeScreenOut(5000);
                Wait(7000);
                Game.Player.Character.Position = new GTA.Math.Vector3(-55.54f, -1111.43f, 26.21f);
                GTA.Math.Vector3 spawnlocation = new GTA.Math.Vector3(-55.54f, -1111.43f, 26.21f);
                Vehicle car = World.CreateVehicle("RADIUS", spawnlocation);
                Game.Player.Character.SetIntoVehicle(car, VehicleSeat.Driver);
                Wait(3000);
                Game.FadeScreenIn(5000);
                Wait(2000);
                UI.Notify("~o~Processing complete!");
                UI.Notify("~g~Enjoy your vehicle!");
                Function.Call(Hash.PLAY_MISSION_COMPLETE_AUDIO);
            }
        };

        //VANS HEADER
        var vanheader = new UIMenuItem("~r~Vans - ~g~$900", "View the following vans available for rent");
        SubRentalServices.AddItem(vanheader);
        SubRentalServices.OnItemSelect += (sender, item, index) =>
        {
            if (item == vanheader)
            {

            }
        };

        //Youga
        var Youga = new UIMenuItem("Rent Youga - $900", "Rent the Youga from our rental agency for only $3,500!");
        SubRentalServices.AddItem(Youga);
        SubRentalServices.OnItemSelect += (sender, item, index) =>
        {
            if (item == Youga)
            {
                //MONEY TAKEN
                Game.Player.Money -= 900;
                UI.Notify("~o~Processing request...");

                //SCREEN FADE AND TELEPORT
                Game.FadeScreenOut(5000);
                Wait(7000);
                Game.Player.Character.Position = new GTA.Math.Vector3(-55.54f, -1111.43f, 26.21f);
                GTA.Math.Vector3 spawnlocation = new GTA.Math.Vector3(-55.54f, -1111.43f, 26.21f);
                Vehicle car = World.CreateVehicle("YOUGA", spawnlocation);
                Game.Player.Character.SetIntoVehicle(car, VehicleSeat.Driver);
                Wait(3000);
                Game.FadeScreenIn(5000);
                Wait(2000);
                UI.Notify("~o~Processing complete!");
                UI.Notify("~g~Enjoy your vehicle!");
                Function.Call(Hash.PLAY_MISSION_COMPLETE_AUDIO);
            }
        };

        //Camper
        var Camper = new UIMenuItem("Rent Camper - $900", "Rent the Camper from our rental agency for only $3,500!");
        SubRentalServices.AddItem(Camper);
        SubRentalServices.OnItemSelect += (sender, item, index) =>
        {
            if (item == Camper)
            {
                //MONEY TAKEN
                Game.Player.Money -= 900;
                UI.Notify("~o~Processing request...");

                //SCREEN FADE AND TELEPORT
                Game.FadeScreenOut(5000);
                Wait(7000);
                Game.Player.Character.Position = new GTA.Math.Vector3(-55.54f, -1111.43f, 26.21f);
                GTA.Math.Vector3 spawnlocation = new GTA.Math.Vector3(-55.54f, -1111.43f, 26.21f);
                Vehicle car = World.CreateVehicle("CAMPER", spawnlocation);
                Game.Player.Character.SetIntoVehicle(car, VehicleSeat.Driver);
                Wait(3000);
                Game.FadeScreenIn(5000);
                Wait(2000);
                UI.Notify("~o~Processing complete!");
                UI.Notify("~g~Enjoy your vehicle!");
                Function.Call(Hash.PLAY_MISSION_COMPLETE_AUDIO);
            }
        };

        //Speedo
        var Speedo = new UIMenuItem("Rent Speedo - $900", "Rent the Speedo from our rental agency for only $3,500!");
        SubRentalServices.AddItem(Speedo);
        SubRentalServices.OnItemSelect += (sender, item, index) =>
        {
            if (item == Speedo)
            {
                //MONEY TAKEN
                Game.Player.Money -= 900;
                UI.Notify("~o~Processing request...");

                //SCREEN FADE AND TELEPORT
                Game.FadeScreenOut(5000);
                Wait(7000);
                Game.Player.Character.Position = new GTA.Math.Vector3(-55.54f, -1111.43f, 26.21f);
                GTA.Math.Vector3 spawnlocation = new GTA.Math.Vector3(-55.54f, -1111.43f, 26.21f);
                Vehicle car = World.CreateVehicle("SPEEDO", spawnlocation);
                Game.Player.Character.SetIntoVehicle(car, VehicleSeat.Driver);
                Wait(3000);
                Game.FadeScreenIn(5000);
                Wait(2000);
                UI.Notify("~o~Processing complete!");
                UI.Notify("~g~Enjoy your vehicle!");
                Function.Call(Hash.PLAY_MISSION_COMPLETE_AUDIO);
            }
        };

        //Minivan
        var Minivan = new UIMenuItem("Rent Minivan - $900", "Rent the Minivan from our rental agency for only $3,500!");
        SubRentalServices.AddItem(Minivan);
        SubRentalServices.OnItemSelect += (sender, item, index) =>
        {
            if (item == Minivan)
            {
                //MONEY TAKEN
                Game.Player.Money -= 900;
                UI.Notify("~o~Processing request...");

                //SCREEN FADE AND TELEPORT
                Game.FadeScreenOut(5000);
                Wait(7000);
                Game.Player.Character.Position = new GTA.Math.Vector3(-55.54f, -1111.43f, 26.21f);
                GTA.Math.Vector3 spawnlocation = new GTA.Math.Vector3(-55.54f, -1111.43f, 26.21f);
                Vehicle car = World.CreateVehicle("MINVAN", spawnlocation);
                Game.Player.Character.SetIntoVehicle(car, VehicleSeat.Driver);
                Wait(3000);
                Game.FadeScreenIn(5000);
                Wait(2000);
                UI.Notify("~o~Processing complete!");
                UI.Notify("~g~Enjoy your vehicle!");
                Function.Call(Hash.PLAY_MISSION_COMPLETE_AUDIO);
            }
        };
    } //DONE

    public void TravelServices(UIMenu menu)
    {
        var SubTravelServices = _menuPool.AddSubMenu(menu, "Travel Services");
        for (int i = 0; i < 1; i++) ;

        //RSA Garage LS
        var rsahq = new UIMenuItem("Roadside Assistance Headquarters (LS)", "Los Santos Roadside Assistance Headquarters located in East Vinewood.");
        SubTravelServices.AddItem(rsahq);
        SubTravelServices.OnItemSelect += (sender, item, index) =>
        {
            if (item == rsahq)
            {

                //SET
                GTA.Math.Vector3 spawnlocation = World.GetNextPositionOnStreet(Game.Player.Character.Position.Around(15f));
                string taxidriver = "a_m_m_skidrow_01";
                Ped taxidrivermodel = GTA.World.CreatePed(taxidriver, spawnlocation);
                Ped player = Game.Player.Character;
                GTA.Math.Vector3 playerlocation = Game.Player.Character.Position.Around(5f);

                //Pick Player Up
                Vehicle taxi = World.CreateVehicle("TAXI", spawnlocation);
                taxi.IsPersistent = true;
                taxidrivermodel.SetIntoVehicle(taxi, VehicleSeat.Driver);
                Function.Call(Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE, taxidrivermodel, taxi, playerlocation.X, playerlocation.Y, playerlocation.Z, 30f, 786603, 1f);
                UI.Notify("~g~Your cab is ready!");
                Wait(3000);
                taxi.LockStatus = VehicleLockStatus.LockedForPlayer;
                taxi.LockStatus = VehicleLockStatus.StickPlayerInside;

                //SET PLAYER IN CAR & TELEPORTS
                Game.Player.CanControlCharacter = false;
                Game.Player.Character.SetIntoVehicle(taxi, VehicleSeat.LeftRear);
                Game.FadeScreenOut(5000);
                Wait(7000);
                taxi.Position = new GTA.Math.Vector3(533.87f, -181.77f, 54.07f);
                taxidrivermodel.Position = new GTA.Math.Vector3(533.87f, -181.77f, 54.07f);
                taxidrivermodel.SetIntoVehicle(taxi, VehicleSeat.Driver);
                Game.Player.Character.Position = new GTA.Math.Vector3(533.87f, -181.77f, 54.07f);
                Game.Player.Character.SetIntoVehicle(taxi, VehicleSeat.LeftRear);
                Wait(3000);
                Game.FadeScreenIn(5000);
                Wait(5000);
                Game.Player.CanControlCharacter = true;
                UI.Notify("~g~Your fare as been paid, you are free to leave.");
                Wait(10000);
                if (Game.Player.Character.IsInTaxi)
                {
                    Game.Player.Character.Position = new GTA.Math.Vector3(533.87f, -181.77f, 54.07f);
                }
                Function.Call(Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE, taxidrivermodel, taxi, -221.749908f, -1158.249756f, 23.040998f, 30f, 786603, 1f);
                Wait(7000);
                taxidrivermodel.Delete();
                taxi.Delete();
                
            }
        };

        //RSA Garage Route 15
        var rsa15 = new UIMenuItem("Roadside Assistance Route 15 Garage", "Roadside Assistance garage located at the fuel station on Route 15.");
        SubTravelServices.AddItem(rsa15);
        SubTravelServices.OnItemSelect += (sender, item, index) =>
        {
            if (item == rsa15)
            {
                //SET
                GTA.Math.Vector3 spawnlocation = World.GetNextPositionOnStreet(Game.Player.Character.Position.Around(15f));
                string taxidriver = "a_m_m_skidrow_01";
                Ped taxidrivermodel = GTA.World.CreatePed(taxidriver, spawnlocation);
                Ped player = Game.Player.Character;
                GTA.Math.Vector3 playerlocation = Game.Player.Character.Position.Around(5f);

                //Pick Player Up
                Vehicle taxi = World.CreateVehicle("TAXI", spawnlocation);
                taxi.IsPersistent = true;
                taxidrivermodel.SetIntoVehicle(taxi, VehicleSeat.Driver);
                Function.Call(Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE, taxidrivermodel, taxi, playerlocation.X, playerlocation.Y, playerlocation.Z, 30f, 786603, 1f);
                UI.Notify("~g~Your cab is ready!");
                Wait(3000);
                taxi.LockStatus = VehicleLockStatus.LockedForPlayer;
                taxi.LockStatus = VehicleLockStatus.StickPlayerInside;

                //SET PLAYER IN CAR & TELEPORTS
                Game.Player.CanControlCharacter = false;
                Game.Player.Character.SetIntoVehicle(taxi, VehicleSeat.LeftRear);
                Game.FadeScreenOut(5000);
                Wait(7000);
                taxi.Position = new GTA.Math.Vector3(2577.71f, 447.04f, 108.23f);
                taxidrivermodel.Position = new GTA.Math.Vector3(2577.71f, 447.04f, 108.23f);
                taxidrivermodel.SetIntoVehicle(taxi, VehicleSeat.Driver);
                Game.Player.Character.Position = new GTA.Math.Vector3(2577.71f, 447.04f, 108.23f);
                Game.Player.Character.SetIntoVehicle(taxi, VehicleSeat.LeftRear);
                Wait(3000);
                Game.FadeScreenIn(5000);
                Wait(5000);
                Game.Player.CanControlCharacter = true;
                UI.Notify("~g~Your fare as been paid, you are free to leave.");
                Wait(10000);
                if (Game.Player.Character.IsInTaxi)
                {
                    Game.Player.Character.Position = new GTA.Math.Vector3(2577.71f, 447.04f, 108.23f);
                }
                Function.Call(Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE, taxidrivermodel, taxi, -221.749908f, -1158.249756f, 23.040998f, 30f, 786603, 1f);
                Wait(7000);
                taxidrivermodel.Delete();
                taxi.Delete();
            }
        };

        //RSA Garage Sandy Shores
        var rsass = new UIMenuItem("Roadside Assistance Sandy Shores Garage", "Roadside Assistance garage located in Sandy Shores.");
        SubTravelServices.AddItem(rsass);
        SubTravelServices.OnItemSelect += (sender, item, index) =>
        {
            if (item == rsass)
            {
                //SET
                GTA.Math.Vector3 spawnlocation = World.GetNextPositionOnStreet(Game.Player.Character.Position.Around(15f));
                string taxidriver = "a_m_m_skidrow_01";
                Ped taxidrivermodel = GTA.World.CreatePed(taxidriver, spawnlocation);
                Ped player = Game.Player.Character;
                GTA.Math.Vector3 playerlocation = Game.Player.Character.Position.Around(5f);

                //Pick Player Up
                Vehicle taxi = World.CreateVehicle("TAXI", spawnlocation);
                taxi.IsPersistent = true;
                taxidrivermodel.SetIntoVehicle(taxi, VehicleSeat.Driver);
                Function.Call(Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE, taxidrivermodel, taxi, playerlocation.X, playerlocation.Y, playerlocation.Z, 30f, 786603, 1f);
                UI.Notify("~g~Your cab is ready!");
                Wait(3000);
                taxi.LockStatus = VehicleLockStatus.LockedForPlayer;
                taxi.LockStatus = VehicleLockStatus.StickPlayerInside;

                //SET PLAYER IN CAR & TELEPORTS
                Game.Player.CanControlCharacter = false;
                Game.Player.Character.SetIntoVehicle(taxi, VehicleSeat.LeftRear);
                Game.FadeScreenOut(5000);
                Wait(7000);
                taxi.Position = new GTA.Math.Vector3(2391.63f, 3111.73f, 48.14f);
                taxidrivermodel.Position = new GTA.Math.Vector3(2391.63f, 3111.73f, 48.14f);
                taxidrivermodel.SetIntoVehicle(taxi, VehicleSeat.Driver);
                Game.Player.Character.Position = new GTA.Math.Vector3(2391.63f, 3111.73f, 48.14f);
                Game.Player.Character.SetIntoVehicle(taxi, VehicleSeat.LeftRear);
                Wait(3000);
                Game.FadeScreenIn(5000);
                Wait(5000);
                Game.Player.CanControlCharacter = true;
                UI.Notify("~g~Your fare as been paid, you are free to leave.");
                Wait(10000);
                if (Game.Player.Character.IsInTaxi)
                {
                    Game.Player.Character.Position = new GTA.Math.Vector3(2391.63f, 3111.73f, 48.14f);
                }
                Function.Call(Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE, taxidrivermodel, taxi, -221.749908f, -1158.249756f, 23.040998f, 30f, 786603, 1f);
                Wait(7000);
                taxidrivermodel.Delete();
                taxi.Delete();
            }
        };

        //RSA Garage Paleto Bay
        var rsapb = new UIMenuItem("Roadside Assistance Paleto Bay Garage", "Roadside Assistance garage located in Paleto Bay.");
        SubTravelServices.AddItem(rsapb);
        SubTravelServices.OnItemSelect += (sender, item, index) =>
        {
            if (item == rsapb)
            {
                //COORDS
                //Game.Player.Character.Position = new GTA.Math.Vector3(-196.40f, 6304.97f, 31.49f);

                //SET
                GTA.Math.Vector3 spawnlocation = World.GetNextPositionOnStreet(Game.Player.Character.Position.Around(15f));
                string taxidriver = "a_m_m_skidrow_01";
                Ped taxidrivermodel = GTA.World.CreatePed(taxidriver, spawnlocation);
                Ped player = Game.Player.Character;
                GTA.Math.Vector3 playerlocation = Game.Player.Character.Position.Around(5f);

                //Pick Player Up
                Vehicle taxi = World.CreateVehicle("TAXI", spawnlocation);
                taxi.IsPersistent = true;
                taxidrivermodel.SetIntoVehicle(taxi, VehicleSeat.Driver);
                Function.Call(Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE, taxidrivermodel, taxi, playerlocation.X, playerlocation.Y, playerlocation.Z, 30f, 786603, 1f);
                UI.Notify("~g~Your cab is ready!");
                Wait(3000);
                taxi.LockStatus = VehicleLockStatus.LockedForPlayer;
                taxi.LockStatus = VehicleLockStatus.StickPlayerInside;

                //SET PLAYER IN CAR & TELEPORTS
                Game.Player.CanControlCharacter = false;
                Game.Player.Character.SetIntoVehicle(taxi, VehicleSeat.LeftRear);
                Game.FadeScreenOut(5000);
                Wait(7000);
                taxi.Position = new GTA.Math.Vector3(-196.40f, 6304.97f, 31.49f);
                taxidrivermodel.Position = new GTA.Math.Vector3(-196.40f, 6304.97f, 31.49f);
                taxidrivermodel.SetIntoVehicle(taxi, VehicleSeat.Driver);
                Game.Player.Character.Position = new GTA.Math.Vector3(-196.40f, 6304.97f, 31.49f);
                Game.Player.Character.SetIntoVehicle(taxi, VehicleSeat.LeftRear);
                Wait(3000);
                Game.FadeScreenIn(5000);
                Wait(5000);
                Game.Player.CanControlCharacter = true;
                UI.Notify("~g~Your fare as been paid, you are free to leave.");
                Wait(10000);
                if (Game.Player.Character.IsInTaxi)
                {
                    Game.Player.Character.Position = new GTA.Math.Vector3(-196.40f, 6304.97f, 31.49f);
                }
                Function.Call(Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE, taxidrivermodel, taxi, -221.749908f, -1158.249756f, 23.040998f, 30f, 786603, 1f);
                Wait(7000);
                taxidrivermodel.Delete();
                taxi.Delete();
            }
        };

        //Mors Mutual Garage
        var mmi = new UIMenuItem("Mors Mutual Insurance", "Mors Mutual Insurance building located in South Los Santos.");
        SubTravelServices.AddItem(mmi);
        SubTravelServices.OnItemSelect += (sender, item, index) =>
        {
            if (item == mmi)
            {
                //SET
                GTA.Math.Vector3 spawnlocation = World.GetNextPositionOnStreet(Game.Player.Character.Position.Around(15f));
                string taxidriver = "a_m_m_skidrow_01";
                Ped taxidrivermodel = GTA.World.CreatePed(taxidriver, spawnlocation);
                Ped player = Game.Player.Character;
                GTA.Math.Vector3 playerlocation = Game.Player.Character.Position.Around(5f);

                //Pick Player Up
                Vehicle taxi = World.CreateVehicle("TAXI", spawnlocation);
                taxi.IsPersistent = true;
                taxidrivermodel.SetIntoVehicle(taxi, VehicleSeat.Driver);
                Function.Call(Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE, taxidrivermodel, taxi, playerlocation.X, playerlocation.Y, playerlocation.Z, 30f, 786603, 1f);
                UI.Notify("~g~Your cab is ready!");
                Wait(3000);
                taxi.LockStatus = VehicleLockStatus.LockedForPlayer;
                taxi.LockStatus = VehicleLockStatus.StickPlayerInside;

                //SET PLAYER IN CAR & TELEPORTS
                Game.Player.CanControlCharacter = false;
                Game.Player.Character.SetIntoVehicle(taxi, VehicleSeat.LeftRear);
                Game.FadeScreenOut(5000);
                Wait(7000);
                taxi.Position = new GTA.Math.Vector3(-221.749908f, -1158.249756f, 23.040998f);
                taxidrivermodel.Position = new GTA.Math.Vector3(-221.749908f, -1158.249756f, 23.040998f);
                taxidrivermodel.SetIntoVehicle(taxi, VehicleSeat.Driver);
                Game.Player.Character.Position = new GTA.Math.Vector3(-221.749908f, -1158.249756f, 23.040998f);
                Game.Player.Character.SetIntoVehicle(taxi, VehicleSeat.LeftRear);
                Wait(3000);
                Game.FadeScreenIn(5000);
                Wait(5000);
                Game.Player.CanControlCharacter = true;
                UI.Notify("~g~Your fare as been paid, you are free to leave.");
                Wait(10000);
                if (Game.Player.Character.IsInTaxi)
                {
                    Game.Player.Character.Position = new GTA.Math.Vector3(-221.749908f, -1158.249756f, 23.040998f);
                }
                Function.Call(Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE, taxidrivermodel, taxi, -196.40f, 6304.97f, 31.49f, 30f, 786603, 1f);
                Wait(7000);
                taxidrivermodel.Delete();
                taxi.Delete();
            }
        };
        
        //RSA Industrial Lot
        var rsalot = new UIMenuItem("Roadside Assistance Industrial Lot", "Roadside Assistance Industrial Lot located in East Los Santos.");
        SubTravelServices.AddItem(rsalot);
        SubTravelServices.OnItemSelect += (sender, item, index) =>
        {
            if (item == rsalot)
            {
                //SET
                GTA.Math.Vector3 spawnlocation = World.GetNextPositionOnStreet(Game.Player.Character.Position.Around(15f));
                string taxidriver = "a_m_m_skidrow_01";
                Ped taxidrivermodel = GTA.World.CreatePed(taxidriver, spawnlocation);
                Ped player = Game.Player.Character;
                GTA.Math.Vector3 playerlocation = Game.Player.Character.Position.Around(5f);

                //Pick Player Up
                Vehicle taxi = World.CreateVehicle("TAXI", spawnlocation);
                taxi.IsPersistent = true;
                taxidrivermodel.SetIntoVehicle(taxi, VehicleSeat.Driver);
                Function.Call(Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE, taxidrivermodel, taxi, playerlocation.X, playerlocation.Y, playerlocation.Z, 30f, 786603, 1f);
                UI.Notify("~g~Your cab is ready!");
                Wait(3000);
                taxi.LockStatus = VehicleLockStatus.LockedForPlayer;
                taxi.LockStatus = VehicleLockStatus.StickPlayerInside;

                //SET PLAYER IN CAR & TELEPORTS
                Game.Player.CanControlCharacter = false;
                Game.Player.Character.SetIntoVehicle(taxi, VehicleSeat.LeftRear);
                Game.FadeScreenOut(5000);
                Wait(7000);
                taxi.Position = new GTA.Math.Vector3(990.01f, -2499.94f, 28.30f);
                taxidrivermodel.Position = new GTA.Math.Vector3(990.01f, -2499.94f, 28.30f);
                taxidrivermodel.SetIntoVehicle(taxi, VehicleSeat.Driver);
                Game.Player.Character.Position = new GTA.Math.Vector3(990.01f, -2499.94f, 28.30f);
                Game.Player.Character.SetIntoVehicle(taxi, VehicleSeat.LeftRear);
                Wait(3000);
                Game.FadeScreenIn(5000);
                Wait(5000);
                Game.Player.CanControlCharacter = true;
                UI.Notify("~g~Your fare as been paid, you are free to leave.");
                Wait(10000);
                if (Game.Player.Character.IsInTaxi)
                {
                    Game.Player.Character.Position = new GTA.Math.Vector3(990.01f, -2499.94f, 28.30f);
                }
                Function.Call(Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE, taxidrivermodel, taxi, -221.749908f, -1158.249756f, 23.040998f, 30f, 786603, 1f);
                Wait(7000);
                taxidrivermodel.Delete();
                taxi.Delete();
            }
        };
    } //DONE BUT CAN BE ADDED TO

    public void WorkerMenu(UIMenu menu)
    {
        var SubWorkerMenu = _menuPool.AddSubMenu(menu, "Worker Menu");
        for (int i = 0; i < 1; i++) ;

        //Road Cone 1A
        var roadcone1A = new UIMenuItem("~o~Road Cone (1A)", "");
        SubWorkerMenu.AddItem(roadcone1A);
        SubWorkerMenu.OnItemSelect += (sender, item, index) =>
        {
            if (item == roadcone1A)
            {
                Prop obj = World.CreateProp("prop_roadcone01a", Game.Player.Character.Position * 1, true, true);
                obj.IsCollisionProof = true;
                obj.HasCollision = true;
            }
        };

        //Road Cone 1B
        var roadcone1B = new UIMenuItem("~o~Road Cone (1B)", "");
        SubWorkerMenu.AddItem(roadcone1B);
        SubWorkerMenu.OnItemSelect += (sender, item, index) =>
        {
            if (item == roadcone1B)
            {
                Prop obj = World.CreateProp("prop_roadcone01b", Game.Player.Character.Position * 1, true, true);
                obj.IsCollisionProof = true;
                obj.HasCollision = true;
            }
        };

        //Road Cone 1C
        var roadcone1C = new UIMenuItem("~o~Road Cone (1C)", "");
        SubWorkerMenu.AddItem(roadcone1C);
        SubWorkerMenu.OnItemSelect += (sender, item, index) =>
        {
            if (item == roadcone1C)
            {
                Prop obj = World.CreateProp("prop_roadcone01c", Game.Player.Character.Position * 1, true, true);
                obj.IsCollisionProof = true;
                obj.HasCollision = true;
            }
        };

        //Road Cone 2A
        var roadcone2A = new UIMenuItem("~o~Road Cone (2A)", "");
        SubWorkerMenu.AddItem(roadcone2A);
        SubWorkerMenu.OnItemSelect += (sender, item, index) =>
        {
            if (item == roadcone2A)
            {
                Prop obj = World.CreateProp("prop_roadcone02a", Game.Player.Character.Position * 1, true, true);
                obj.IsCollisionProof = true;
                obj.HasCollision = true;
            }
        };

        //Road Cone 2B
        var roadcone2B = new UIMenuItem("~o~Road Cone (2B)", "");
        SubWorkerMenu.AddItem(roadcone2B);
        SubWorkerMenu.OnItemSelect += (sender, item, index) =>
        {
            if (item == roadcone2B)
            {
                Prop obj = World.CreateProp("prop_roadcone02b", Game.Player.Character.Position * 1, true, true);
                obj.IsCollisionProof = true;
                obj.HasCollision = true;
            }
        };

        //Road Cone 2C
        var roadcone2C = new UIMenuItem("~o~Road Cone (2C)", "");
        SubWorkerMenu.AddItem(roadcone2C);
        SubWorkerMenu.OnItemSelect += (sender, item, index) =>
        {
            if (item == roadcone2C)
            {
                Prop obj = World.CreateProp("prop_roadcone02c", Game.Player.Character.Position * 1, true, true);
                obj.IsCollisionProof = true;
                obj.HasCollision = true;
            }
        };
    }

    public RSA()
    {
        _menuPool = new MenuPool();
        var mainMenu = new UIMenu("RSA", "~y~Mod by Abel Gaming and AlexBoosted");
        _menuPool.Add(mainMenu);
        //_menuPool.SetBannerType("scripts\\RSA-Banner.png");
        RoadSideServices(mainMenu);
        RentalServices(mainMenu);
        TravelServices(mainMenu);
        WorkerMenu(mainMenu);
        _menuPool.RefreshIndex();

        //INI FILE
        config = ScriptSettings.Load("scripts\\RoadsideAssistanceV.ini");
        OpenMenu = config.GetValue<Keys>("Options", "OpenMenu", Keys.F5);
        OnCall = config.GetValue<Keys>("Options", "OnCall", Keys.F1);
        ForceCall = config.GetValue<Keys>("Options", "ForceCall", Keys.F2);
        StreetName = config.GetValue<Keys>("Options", "GetStreetName", Keys.F4);
        Radio = config.GetValue<Keys>("Options", "Radio", Keys.R);
        unit = config.GetValue<string>("Options", "Unit", "TOW1");
        name = config.GetValue<string>("Options", "Name", "John");
        RSAVehicleModel = config.GetValue<string>("Options", "RSAVehicleModel", "LGUARD");

        //BLIPS
        var RSAHQLS = new GTA.Math.Vector3(537.52f, -171.15f, 54.51f);
        var RSAHQLSBLIP = World.CreateBlip(RSAHQLS);
        RSAHQLSBLIP.Sprite = BlipSprite.Business;

        var RSAHQ15 = new GTA.Math.Vector3(2587.64f, 429.33f, 108.61f);
        var RSAHQ15BLIP = World.CreateBlip(RSAHQ15);
        RSAHQ15BLIP.Sprite = BlipSprite.TowTruck;

        var RSAHQSS = new GTA.Math.Vector3(2391.63f, 3111.73f, 48.14f);
        var RSAHQSSBLIP = World.CreateBlip(RSAHQSS);
        RSAHQSSBLIP.Sprite = BlipSprite.TowTruck;

        var RSAHQPB = new GTA.Math.Vector3(-196.40f, 6304.97f, 31.49f);
        var RSAHQPBBLIP = World.CreateBlip(RSAHQPB);
        RSAHQPBBLIP.Sprite = BlipSprite.TowTruck;

        var RSALOT = new GTA.Math.Vector3(990.01f, -2499.94f, 28.30f);
        var RSALOTBLIP = World.CreateBlip(RSALOT);
        RSALOTBLIP.Sprite = BlipSprite.Truck;
        
        //BASE CODE
        Tick += (o, e) => _menuPool.ProcessMenus();
        KeyDown += (o, e) =>
        {
            if (e.KeyCode == OpenMenu && !_menuPool.IsAnyMenuOpen()) // Menu on/off switch
                mainMenu.Visible = !mainMenu.Visible;

            if (e.KeyCode == OnCall)
            {
                //Set Variables
                var rsals = new GTA.Math.Vector3(537.52f, -171.15f, 54.51f); //LOS SANTOS
                var rsa15 = new GTA.Math.Vector3(2587.64f, 429.33f, 108.61f); //ROUTE 15
                var rsass = new GTA.Math.Vector3(2391.63f, 3111.73f, 48.14f); //SANDY SHORES
                var rsapb = new GTA.Math.Vector3(-196.40f, 6304.97f, 31.49f); //PALETO BAY
                
                //RANDOM COORDS
                List<GTA.Math.Vector3> oncallspawn = new List<GTA.Math.Vector3>();
                oncallspawn.Add(rsals);
                oncallspawn.Add(rsa15);
                oncallspawn.Add(rsass);
                oncallspawn.Add(rsapb);
                Random rnd4 = new Random();

                //ON-CALL CODE
                Game.Player.CanControlCharacter = false;
                Game.FadeScreenOut(5000);
                Wait(6000);
                Game.Player.Character.Position = oncallspawn[rnd4.Next(0, 3)];
                Game.Player.ChangeModel("S_M_Y_XMECH_01");
                Wait(3000);
                Game.FadeScreenIn(5000);
                UI.Notify("~g~You are now on call for Roadside Assistance as " + unit + "!");
                UI.Notify("~b~Welcome to RSA, " + name + "!");
                Game.Player.CanControlCharacter = true;
                UI.DrawTexture("scripts\\scanner.png", 0, 0, 15000, new Point(900, 500), new Size(122, 278));
            }
            
            if (e.KeyCode == ForceCall)
            {
                //Random Call Lines
                List<string> callouts = new List<string>();
                callouts.Add("~y~, we have reports of a broken down black Tailgater at the LTD on Grove Street. Please respond as soon as possible.");
                callouts.Add("~y~, Los Santos Police Department just contacted us and informed us a police vehicle is needing towed from the Davis police station.");
                callouts.Add("~y~, can you please respond to a vehicle blocking a fire hydrant at Los Santos Beach.");
                callouts.Add("~y~, can you please respond to a vehicle blocking a fire hydrant at Los Santos Beach.");
                Random rnd3 = new Random();
                UI.Notify("~y~Control to " + unit + callouts[rnd3.Next(0, 3)]);

                if (e.KeyCode == Keys.Y)
                {
                    UI.Notify("You have responded to the call.");
                }
            }
            
            if (e.KeyCode == StreetName)
            {
                UI.Notify(World.GetStreetName(Game.Player.Character.Position));
            }

            if (e.KeyCode == Radio)
            {
                UI.DrawTexture("scripts\\scanner.png", 0, 0, 30000, new Point(900, 500), new Size(122, 278));
            }
        };
    }
}

