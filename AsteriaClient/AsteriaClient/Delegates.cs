using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsteriaClient
{
    public delegate void CreateCharacterHandler(CharacterMgtEventArgs e);
    public delegate void DeleteCharacterHandler(CharacterMgtEventArgs e);
    public delegate void StartCharacterHandler(CharacterMgtEventArgs e);
}
