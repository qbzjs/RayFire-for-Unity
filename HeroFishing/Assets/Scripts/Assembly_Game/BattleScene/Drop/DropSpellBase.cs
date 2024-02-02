using HeroFishing.Main;
using Scoz.Func;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DropBase {
    public virtual float Duration { get; }
    protected DropJsonData _data;
    public DropBase(DropJsonData data) {
        _data = data;
    }

    public abstract void AddDrop();
    public abstract bool PlayDrop();
}

public abstract class DropSpellBase : DropBase {
    protected DropSpellJsonData _spellData;
    protected DropUI _dropUI;
    public DropSpellBase(DropJsonData data, DropSpellJsonData spellData) : base(data) {
        _spellData = spellData;
        _dropUI = BaseUI.GetInstance<DropUI>();
    }

    public override void AddDrop() {
        _dropUI.OnDropAdd(_data.ID);
    }
}
