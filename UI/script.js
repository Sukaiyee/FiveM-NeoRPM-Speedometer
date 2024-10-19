window.onload = function() {
    const speedometer = document.getElementById('speedometer');
    
    // Ensure speedometer is hidden on load
    speedometer.style.display = 'none';

    // Cache DOM elements
    const speedValueElem = document.querySelector('.speed-value');
    const speedUnitElem = document.querySelector('.speed-unit');
    const gearIndicatorElem = document.querySelector('.gear-indicator');
    const fuelBarFillElem = document.querySelector('.fuel-bar-fill-vertical');
    const fuelBarContainer = fuelBarFillElem.parentElement;
    const rpmBarElem = document.querySelector('.rpm-bar');
    const rpmBarContainer = rpmBarElem.parentElement;

    // Variables to store previous values
    let prevSpeed = null;
    let prevUnit = null;
    let prevGear = null;
    let prevFuel = null;
    let prevRpm = null;
    let prevShowSpeedo = false;

    window.addEventListener('message', function(event) {
        const data = event.data;

        if (data.showSpeedo) {
            if (!prevShowSpeedo) {
                speedometer.style.display = 'block';
                prevShowSpeedo = true;
            }

            // Update speed only if it has changed
            const speed = String(data.speed).padStart(3, '0');
            if (speed !== prevSpeed) {
                speedValueElem.innerText = speed;
                prevSpeed = speed;
            }

            // Update unit only if it has changed
            if (data.unit !== prevUnit) {
                speedUnitElem.innerText = data.unit;
                prevUnit = data.unit;
            }

            // Handle gear and RPM display
            if (data.gear !== null && data.rpm !== null) {
                // Show gear and RPM indicators
                gearIndicatorElem.style.display = '';
                rpmBarContainer.style.display = '';

                // Update gear only if it has changed
                const gear = data.gear === 0 ? 'R' : data.gear;
                if (gear !== prevGear) {
                    gearIndicatorElem.innerText = gear;
                    prevGear = gear;
                }

                // Update RPM only if it has changed
                if (data.rpm !== prevRpm) {
                    rpmBarElem.style.width = data.rpm + '%';

                    // Update RPM bar color based on RPM value
                    if (data.rpm > 80) {
                        rpmBarElem.classList.add('high');
                    } else {
                        rpmBarElem.classList.remove('high');
                    }
                    prevRpm = data.rpm;
                }
            } else {
                // Hide gear and RPM indicators
                gearIndicatorElem.style.display = 'none';
                rpmBarContainer.style.display = 'none';

                // Reset previous gear and rpm values
                prevGear = null;
                prevRpm = null;
            }

            // Handle fuel display
            if (data.fuel !== null) {
                fuelBarContainer.style.display = '';
                if (data.fuel !== prevFuel) {
                    fuelBarFillElem.style.height = data.fuel + '%';
                    prevFuel = data.fuel;
                }
            } else {
                // Hide fuel indicator
                fuelBarContainer.style.display = 'none';
                prevFuel = null;
            }
        } else {
            if (prevShowSpeedo) {
                speedometer.style.display = 'none';
                prevShowSpeedo = false;

                // Reset previous values
                prevSpeed = null;
                prevUnit = null;
                prevGear = null;
                prevFuel = null;
                prevRpm = null;
            }
        }
    });
}
