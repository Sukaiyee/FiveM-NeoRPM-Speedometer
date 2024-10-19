using System;
using CitizenFX.Core;
using CitizenFX.Core.Native;

public class Speedometer : BaseScript
{
    private bool useKMH = true;
    private int previousSpeed = -1;
    private int previousGear = -1;
    private int previousRpm = -1;
    private int previousFuelPercent = -1;
    private string previousUnit;
    private bool isSpeedoVisible = false;

    public Speedometer()
    {
        previousUnit = useKMH ? "km/h" : "mph";
        EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
        Tick += SpeedometerTick;
        Tick += ToggleSpeedUnitHandler;
    }

    private void OnClientResourceStart(string resourceName)
    {
        if (API.GetCurrentResourceName() != resourceName) return;
        API.SendNuiMessage("{\"showSpeedo\": false}");
    }

    private async Task SpeedometerTick()
    {
        await Delay(500);
        int player = API.PlayerPedId();

        if (API.IsPedInAnyVehicle(player, false))
        {
            int vehicle = API.GetVehiclePedIsIn(player, false);
            isSpeedoVisible = false;

            while (API.IsPedInAnyVehicle(player, false))
            {
                if (API.IsPauseMenuActive())
                {
                    if (isSpeedoVisible)
                    {
                        API.SendNuiMessage("{\"showSpeedo\": false}");
                        isSpeedoVisible = false;
                    }
                }
                else
                {
                    vehicle = API.GetVehiclePedIsIn(player, false);
                    float speed = API.GetEntitySpeed(vehicle);
                    speed = useKMH ? speed * 3.6f : speed * 2.23694f;
                    int roundedSpeed = (int)Math.Floor(speed + 0.5f);
                    int vehicleClass = API.GetVehicleClass(vehicle);

                    // Set defaults for display options
                    bool showGearAndRPM = ShouldShowGearAndRpm(vehicleClass);
                    bool showFuel = vehicleClass != 13;  // Do not show fuel for cycles

                    // Conditions for boats, helicopters, planes, and cycles
                    if (vehicleClass == 14 || vehicleClass == 15 || vehicleClass == 16)
                    {
                        // Boats, helicopters, and planes: only show speed and fuel
                        showGearAndRPM = false;
                    }
                    else if (vehicleClass == 13)
                    {
                        // Cycles: only show speed
                        showGearAndRPM = false;
                        showFuel = false;
                    }

                    int? rpm = null;
                    int? gear = null;

                    if (showGearAndRPM)
                    {
                        rpm = (int)Math.Floor(API.GetVehicleCurrentRpm(vehicle) * 100 + 0.5);
                        gear = API.GetVehicleCurrentGear(vehicle);
                    }

                    int? fuelPercent = null;
                    if (showFuel)
                    {
                        float maxFuel = API.GetVehicleHandlingFloat(vehicle, "CHandlingData", "fPetrolTankVolume");
                        float currentFuel = API.GetVehicleFuelLevel(vehicle);
                        fuelPercent = (int)Math.Floor((currentFuel / maxFuel) * 100 + 0.5);
                    }

                    if (roundedSpeed != previousSpeed || gear != previousGear || rpm != previousRpm || fuelPercent != previousFuelPercent || !isSpeedoVisible)
                    {
                        // Build JSON message and replace null values with empty strings
                        string jsonData = $"{{\"showSpeedo\": true, \"speed\": {roundedSpeed}, \"gear\": {(gear.HasValue ? gear.Value.ToString() : "null")}, \"rpm\": {(rpm.HasValue ? rpm.Value.ToString() : "null")}, \"fuel\": {(fuelPercent.HasValue ? fuelPercent.Value.ToString() : "null")}, \"unit\": \"{previousUnit}\"}}";
                        API.SendNuiMessage(jsonData);

                        previousSpeed = roundedSpeed;
                        previousGear = gear ?? -1;
                        previousRpm = rpm ?? -1;
                        previousFuelPercent = fuelPercent ?? -1;
                        isSpeedoVisible = true;
                    }
                }
                await Delay(100); // Faster update while in the vehicle
            }

            if (isSpeedoVisible)
            {
                API.SendNuiMessage("{\"showSpeedo\": false}");
                isSpeedoVisible = false;
            }

            ResetPreviousValues();
        }
    }

    private async Task ToggleSpeedUnitHandler()
    {
        while (true) // Continuously check for key presses
        {
            if (API.IsControlJustReleased(0, 303)) // U key
            {
                ToggleSpeedUnit();
                await Delay(100); // Small delay to prevent accidental multiple toggles
            }
            await Delay(0); // Continuously check for key press
        }
    }

    private void ToggleSpeedUnit()
    {
        useKMH = !useKMH;
        previousUnit = useKMH ? "km/h" : "mph";
        API.SendNuiMessage("{\"color\": [255, 255, 0], \"multiline\": true, \"args\": [\"kara_speedo\", \"Speed unit changed to " + previousUnit + "\"]}");

        // Add chat notification for speed unit change
        TriggerEvent("chat:addMessage", new
        {
            color = new[] { 255, 192, 203 }, // Pink color
            multiline = true,
            args = new[] { "Speedometer", $"Speed unit changed to {previousUnit}" }
        });
    }

    private void ResetPreviousValues()
    {
        previousSpeed = -1;
        previousGear = -1;
        previousRpm = -1;
        previousFuelPercent = -1;
    }

    private bool ShouldShowGearAndRpm(int vehicleClass)
    {
        switch (vehicleClass)
        {
            case 0: case 1: case 2: case 3: case 4: case 5: case 6: case 7: case 8:
            case 9: case 10: case 11: case 12: case 17: case 18: case 19: case 20:
            case 22:
                return true;
            case 13: // Cycles
            case 14: // Boats
            case 15: // Helicopters
            case 16: // Planes
                return false;
            default:
                return false;
        }
    }
}
