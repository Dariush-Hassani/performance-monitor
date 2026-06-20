using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Threading;

namespace performance_monitor
{
    public class HardwareMonitor : IDisposable
    {
        private PerformanceCounter cpuTotalCounter;
        private List<PerformanceCounter> cpuCoreCounters;
        private PerformanceCounter cpuPerformanceCounter;
        private double maxCpuClockHz;

        private PerformanceCounter ramAvailableCounter;
        private double totalRamBytes;

        private PerformanceCounter diskReadCounter;
        private PerformanceCounter diskWriteCounter;
        private PerformanceCounter diskIdleCounter;

        public HardwareMonitor()
        {
            InitializeCpu();
            InitializeRam();
            InitializeDisk();
            WarmUp();
        }

        private void InitializeCpu()
        {
            try
            {
                cpuTotalCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");

                cpuCoreCounters = new List<PerformanceCounter>();
                var category = new PerformanceCounterCategory("Processor");
                string[] instances = category.GetInstanceNames();
                foreach (var instance in instances)
                {
                    if (instance != "_Total")
                    {
                        cpuCoreCounters.Add(
                            new PerformanceCounter("Processor", "% Processor Time", instance));
                    }
                }

                cpuPerformanceCounter = new PerformanceCounter(
                    "Processor Information", "% Processor Performance", "_Total");

                maxCpuClockHz = GetMaxCpuClockHz();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to initialize CPU counters: {ex.Message}");
            }
        }

        private double GetMaxCpuClockHz()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher(
                    "SELECT MaxClockSpeed FROM Win32_Processor"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        double maxMHz = Convert.ToDouble(obj["MaxClockSpeed"]);
                        return maxMHz * 1_000_000.0;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to read MaxClockSpeed: {ex.Message}");
            }
            return 0;
        }

        private double GetCpuTemperatureKelvin()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher(
                    @"root\WMI",
                    "SELECT CurrentTemperature FROM MSAcpi_ThermalZoneTemperature"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        double tempKelvinTenths = Convert.ToDouble(obj["CurrentTemperature"]);
                        return tempKelvinTenths / 10.0;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to read CPU temperature: {ex.Message}");
            }
            return 0;
        }

        private void InitializeRam()
        {
            try
            {
                ramAvailableCounter = new PerformanceCounter("Memory", "Available Bytes");
                totalRamBytes = GetTotalRamBytes();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to initialize RAM counters: {ex.Message}");
            }
        }

        private double GetTotalRamBytes()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher(
                    "SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        double totalKB = Convert.ToDouble(obj["TotalVisibleMemorySize"]);
                        return totalKB * 1024.0;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to read total RAM: {ex.Message}");
            }
            return 0;
        }

        private void InitializeDisk()
        {
            try
            {
                diskReadCounter = new PerformanceCounter(
                    "PhysicalDisk", "Disk Read Bytes/sec", "_Total");
                diskWriteCounter = new PerformanceCounter(
                    "PhysicalDisk", "Disk Write Bytes/sec", "_Total");
                diskIdleCounter = new PerformanceCounter(
                    "PhysicalDisk", "% Idle Time", "_Total");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to initialize Disk counters: {ex.Message}");
            }
        }

        private void WarmUp()
        {
            try
            {
                cpuTotalCounter?.NextValue();
                cpuPerformanceCounter?.NextValue();
                ramAvailableCounter?.NextValue();
                diskReadCounter?.NextValue();
                diskWriteCounter?.NextValue();
                diskIdleCounter?.NextValue();

                if (cpuCoreCounters != null)
                {
                    foreach (var counter in cpuCoreCounters)
                        counter.NextValue();
                }

                Thread.Sleep(1000);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed during warm up: {ex.Message}");
            }
        }

        public object GetCpuStats()
        {
            double cpuTotalUsage = SafeNextValue(cpuTotalCounter);

            var coreUsages = new List<double>();
            if (cpuCoreCounters != null)
            {
                foreach (var counter in cpuCoreCounters)
                    coreUsages.Add(SafeNextValue(counter));
            }

            double perfPercent = SafeNextValue(cpuPerformanceCounter);
            double currentClockHz = maxCpuClockHz * (perfPercent / 100.0);

            double temperatureKelvin = GetCpuTemperatureKelvin();

            return new
            {
                totalUsagePercent = cpuTotalUsage,
                coreUsagesPercent = coreUsages,
                currentClockHz = currentClockHz,
                maxClockHz = maxCpuClockHz,
                temperatureKelvin = temperatureKelvin,
                timestamp = DateTime.Now
            };
        }

        public object GetRamStats()
        {
            double availableBytes = SafeNextValue(ramAvailableCounter);
            double usedBytes = totalRamBytes - availableBytes;

            return new
            {
                totalBytes = totalRamBytes,
                usedBytes = usedBytes,
                availableBytes = availableBytes,
                timestamp = DateTime.Now
            };
        }

        public object GetDiskStats()
        {
            double readBytesPerSec = SafeNextValue(diskReadCounter);
            double writeBytesPerSec = SafeNextValue(diskWriteCounter);
            double idleTimePercent = SafeNextValue(diskIdleCounter);
            double activeTimePercent = 100 - idleTimePercent;

            return new
            {
                readBytesPerSec = readBytesPerSec,
                writeBytesPerSec = writeBytesPerSec,
                activeTimePercent = activeTimePercent,
                timestamp = DateTime.Now
            };
        }

        private float SafeNextValue(PerformanceCounter counter)
        {
            try
            {
                return counter?.NextValue() ?? 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to read counter value: {ex.Message}");
                return 0;
            }
        }

        public void Dispose()
        {
            cpuTotalCounter?.Dispose();
            cpuPerformanceCounter?.Dispose();
            ramAvailableCounter?.Dispose();
            diskReadCounter?.Dispose();
            diskWriteCounter?.Dispose();
            diskIdleCounter?.Dispose();

            if (cpuCoreCounters != null)
            {
                foreach (var counter in cpuCoreCounters)
                    counter?.Dispose();
            }
        }
    }
}
