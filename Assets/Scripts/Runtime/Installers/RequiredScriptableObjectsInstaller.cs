using System.Collections.Generic;
using UnityEngine;
using World.Data;
using Zenject;

namespace FirstSettler.Installers
{
    public class RequiredScriptableObjectsInstaller : MonoInstaller
    {
        [SerializeField] private MaterialKeyAndUnityMaterialAssociations _materialKeyAndUnityMaterialAssociations;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo(_materialKeyAndUnityMaterialAssociations.GetType())
                .FromInstance(_materialKeyAndUnityMaterialAssociations).AsSingle();
        }
    }
}
