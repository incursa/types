// Copyright (c) Samuel McAravey
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Incursa;

/// <summary>Defines explicit resolution for ambiguous or nonexistent local times.</summary>
public enum LocalTimeResolution
{
    /// <summary>Reject ambiguous and nonexistent local times.</summary>
    Reject,

    /// <summary>For ambiguity, select the earlier resulting instant.</summary>
    EarlierInstant,

    /// <summary>For ambiguity, select the later resulting instant.</summary>
    LaterInstant,

    /// <summary>For a gap, shift local fields forward by the transition gap.</summary>
    ShiftForward,

    /// <summary>For a gap, shift local fields backward by the transition gap.</summary>
    ShiftBackward,
}
