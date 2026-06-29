# SDRSharp.GpredictConnector Changelog

## v0.4
* fixed command "f" due to SDRSharp 1922 changes (tnx @pizzolante IU7TUY)
* added stub for set_mode/get_mode (m/M commands), awaiting HamLib sdrsharp.c backend implementation (tnx @pizzolante IU7TUY)
* added workflow in github repository
* tested with 
  * gpredict v2.3.37
  * WSJT-X V3.0.1
  * SDRSharp v1.0.0.1922

## v0.3
* added command "f" to get the last set frequency back (tnx @Jansemar)
* added propper return error codes 
* added version display
* tested with 
  * gpredict v2.2.1 and v2.3.20
  * SDRSharp v1.0.0.1671
