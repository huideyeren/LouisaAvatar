namespace XWear.XWearPackage.Editor.Validator
{
    public enum ValidateResultType
    {
        // Common
        Ok,
        CommonErrorMissingRootGameObject,
        CommonErrorNoHierarchy,
        CommonErrorNoRenderer,
        SmrErrorNotFound,
        SmrErrorMissingRootBone,
        SmrErrorNotContainsRootBone,
        SmrErrorInactiveWeightedBones,
        SmrWarningContainsNulBone,
        SmrWarningZeroWeightBone,

        // For Avatar
        AvatarErrorNotHumanoid,
        AvatarErrorNotFoundAnimator,
        AvatarErrorContainsSameNameHierarchy,

        // For Wear
        WearErrorContainsNullHumanoidMapComponent,
        WearErrorInvalidHumanoidComponent,
        WearErrorNotFoundHumanoidComponent,
        WearErrorEmptyHumanoidMap,

        // For Accessory
        AccessoryWarningNotFoundAccessoryMap,
        AccessoryWarningMapBoneIsEmpty,
        AccessoryIsNotSupported
    }
}
