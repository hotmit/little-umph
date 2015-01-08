# Version Marker
* NET20 - Framework 2.0
* NET35
  * NET35; NET35_OR_GREATER;
  ```c#
  #if NET35_OR_GREATER
  using System.Linq;
  #endif
  ```

* NET40
  * NET35_OR_GREATER; NET40; NET40_OR_GREATER;
* NET45
  * NET35_OR_GREATER; NET40_OR_GREATER; NET45; NET45_OR_GREATER;
