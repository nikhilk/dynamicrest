// IRestUriTransformer.cs
// DynamicRest provides REST service access using C# 4.0 dynamic programming.
// The latest information and code for the project can be found at
// https://github.com/NikhilK/dynamicrest
//
// This project is licensed under the BSD license. See the License.txt file for
// more information.
//

using System;

namespace DynamicRest {

    public interface IRestUriTransformer {

        Uri TransformUri(Uri uri);
    }
}
