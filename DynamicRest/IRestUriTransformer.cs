// IRestUriTransformer.cs
//

using System;

namespace DynamicRest {

    public interface IRestUriTransformer {

        Uri TransformUri(Uri uri);
    }
}
