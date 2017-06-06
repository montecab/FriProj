#!/bin/sh
set -e

RESOURCES_TO_COPY=${PODS_ROOT}/resources-to-copy-${TARGETNAME}.txt
> "$RESOURCES_TO_COPY"

install_resource()
{
  case $1 in
    *.storyboard)
      echo "ibtool --reference-external-strings-file --errors --warnings --notices --output-format human-readable-text --compile ${CONFIGURATION_BUILD_DIR}/${UNLOCALIZED_RESOURCES_FOLDER_PATH}/`basename \"$1\" .storyboard`.storyboardc ${PODS_ROOT}/$1 --sdk ${SDKROOT}"
      ibtool --reference-external-strings-file --errors --warnings --notices --output-format human-readable-text --compile "${CONFIGURATION_BUILD_DIR}/${UNLOCALIZED_RESOURCES_FOLDER_PATH}/`basename \"$1\" .storyboard`.storyboardc" "${PODS_ROOT}/$1" --sdk "${SDKROOT}"
      ;;
    *.xib)
        echo "ibtool --reference-external-strings-file --errors --warnings --notices --output-format human-readable-text --compile ${CONFIGURATION_BUILD_DIR}/${UNLOCALIZED_RESOURCES_FOLDER_PATH}/`basename \"$1\" .xib`.nib ${PODS_ROOT}/$1 --sdk ${SDKROOT}"
      ibtool --reference-external-strings-file --errors --warnings --notices --output-format human-readable-text --compile "${CONFIGURATION_BUILD_DIR}/${UNLOCALIZED_RESOURCES_FOLDER_PATH}/`basename \"$1\" .xib`.nib" "${PODS_ROOT}/$1" --sdk "${SDKROOT}"
      ;;
    *.framework)
      echo "mkdir -p ${CONFIGURATION_BUILD_DIR}/${FRAMEWORKS_FOLDER_PATH}"
      mkdir -p "${CONFIGURATION_BUILD_DIR}/${FRAMEWORKS_FOLDER_PATH}"
      echo "rsync -av ${PODS_ROOT}/$1 ${CONFIGURATION_BUILD_DIR}/${FRAMEWORKS_FOLDER_PATH}"
      rsync -av "${PODS_ROOT}/$1" "${CONFIGURATION_BUILD_DIR}/${FRAMEWORKS_FOLDER_PATH}"
      ;;
    *.xcdatamodel)
      echo "xcrun momc \"${PODS_ROOT}/$1\" \"${CONFIGURATION_BUILD_DIR}/${UNLOCALIZED_RESOURCES_FOLDER_PATH}/`basename "$1"`.mom\""
      xcrun momc "${PODS_ROOT}/$1" "${CONFIGURATION_BUILD_DIR}/${UNLOCALIZED_RESOURCES_FOLDER_PATH}/`basename "$1" .xcdatamodel`.mom"
      ;;
    *.xcdatamodeld)
      echo "xcrun momc \"${PODS_ROOT}/$1\" \"${CONFIGURATION_BUILD_DIR}/${UNLOCALIZED_RESOURCES_FOLDER_PATH}/`basename "$1" .xcdatamodeld`.momd\""
      xcrun momc "${PODS_ROOT}/$1" "${CONFIGURATION_BUILD_DIR}/${UNLOCALIZED_RESOURCES_FOLDER_PATH}/`basename "$1" .xcdatamodeld`.momd"
      ;;
    *.xcassets)
      ;;
    /*)
      echo "$1"
      echo "$1" >> "$RESOURCES_TO_COPY"
      ;;
    *)
      echo "${PODS_ROOT}/$1"
      echo "${PODS_ROOT}/$1" >> "$RESOURCES_TO_COPY"
      ;;
  esac
}
          install_resource "UrbanAirship-iOS-SDK/Airship/UI/Default/Common/Resources/de.lproj/UAInteractiveNotifications.strings"
                    install_resource "UrbanAirship-iOS-SDK/Airship/UI/Default/Common/Resources/en.lproj/UAInteractiveNotifications.strings"
                    install_resource "UrbanAirship-iOS-SDK/Airship/UI/Default/Common/Resources/es.lproj/UAInteractiveNotifications.strings"
                    install_resource "UrbanAirship-iOS-SDK/Airship/UI/Default/Common/Resources/fr.lproj/UAInteractiveNotifications.strings"
                    install_resource "UrbanAirship-iOS-SDK/Airship/UI/Default/Common/Resources/it.lproj/UAInteractiveNotifications.strings"
                    install_resource "UrbanAirship-iOS-SDK/Airship/UI/Default/Common/Resources/ja.lproj/UAInteractiveNotifications.strings"
                    install_resource "UrbanAirship-iOS-SDK/Airship/UI/Default/Common/Resources/pt.lproj/UAInteractiveNotifications.strings"
                    install_resource "UrbanAirship-iOS-SDK/Airship/UI/Default/Common/Resources/Settings.bundle/en.lproj/Root.strings"
                    install_resource "UrbanAirship-iOS-SDK/Airship/UI/Default/Common/Resources/zh-Hans.lproj/UAInteractiveNotifications.strings"
                    install_resource "UrbanAirship-iOS-SDK/Airship/UI/Default/Common/Resources/zh-Hant.lproj/UAInteractiveNotifications.strings"
                    install_resource "UrbanAirship-iOS-SDK/Airship/UI/Default/Inbox/Resources/en.lproj/UAInboxUI.strings"
                    install_resource "UrbanAirship-iOS-SDK/Airship/UI/Default/Inbox/Resources/Shared/list-image-placeholder.png"
                    install_resource "UrbanAirship-iOS-SDK/Airship/UI/Default/Inbox/Resources/Shared/list-image-placeholder@2x.png"
                    install_resource "UrbanAirship-iOS-SDK/Airship/UI/Default/Inbox/Resources/Shared/UAInboxMessageListCell.xib"
                    install_resource "UrbanAirship-iOS-SDK/Airship/UI/Default/Inbox/Resources/Shared/UAInboxMessageListController.xib"
                    install_resource "UrbanAirship-iOS-SDK/Airship/UI/Default/Inbox/Resources/Shared/UAInboxMessageViewController.xib"
                    install_resource "UrbanAirship-iOS-SDK/Airship/UI/Default/Push/Resources/en.lproj/UAPushUI.strings"
                    install_resource "UrbanAirship-iOS-SDK/Airship/UI/Default/Push/Resources/Shared/bottom-detail.png"
                    install_resource "UrbanAirship-iOS-SDK/Airship/UI/Default/Push/Resources/Shared/bottom-detail@2x.png"
                    install_resource "UrbanAirship-iOS-SDK/Airship/UI/Default/Push/Resources/Shared/middle-detail.png"
                    install_resource "UrbanAirship-iOS-SDK/Airship/UI/Default/Push/Resources/Shared/middle-detail@2x.png"
                    install_resource "UrbanAirship-iOS-SDK/Airship/UI/Default/Push/Resources/Shared/top-detail.png"
                    install_resource "UrbanAirship-iOS-SDK/Airship/UI/Default/Push/Resources/Shared/top-detail@2x.png"
                    install_resource "UrbanAirship-iOS-SDK/Airship/UI/Default/Push/Resources/Shared/UALocationSettingsViewController.xib"
                    install_resource "UrbanAirship-iOS-SDK/Airship/UI/Default/Push/Resources/Shared/UAMapPresentationViewController.xib"
                    install_resource "UrbanAirship-iOS-SDK/Airship/UI/Default/Push/Resources/Shared/UAPushMoreSettingsView.xib"
                    install_resource "UrbanAirship-iOS-SDK/Airship/UI/Default/Push/Resources/Shared/UAPushSettingsAddTagViewController.xib"
                    install_resource "UrbanAirship-iOS-SDK/Airship/UI/Default/Push/Resources/Shared/UAPushSettingsAliasView.xib"
                    install_resource "UrbanAirship-iOS-SDK/Airship/UI/Default/Push/Resources/Shared/UAPushSettingsChannelInfoViewController.xib"
                    install_resource "UrbanAirship-iOS-SDK/Airship/UI/Default/Push/Resources/Shared/UAPushSettingsSoundsViewController.xib"
                    install_resource "UrbanAirship-iOS-SDK/Airship/UI/Default/Push/Resources/Shared/UAPushSettingsTagsViewController.xib"
                    install_resource "UrbanAirship-iOS-SDK/Airship/UI/Default/Push/Resources/Shared/UAPushSettingsTokenView.xib"
                    install_resource "UrbanAirship-iOS-SDK/Airship/UI/Default/Push/Resources/Shared/UAPushSettingsUserInfoView.xib"
                    install_resource "UrbanAirship-iOS-SDK/Airship/UI/Default/Push/Resources/Shared/UAPushSettingsView.xib"
          
rsync -avr --copy-links --no-relative --exclude '*/.svn/*' --files-from="$RESOURCES_TO_COPY" / "${CONFIGURATION_BUILD_DIR}/${UNLOCALIZED_RESOURCES_FOLDER_PATH}"
if [[ "${ACTION}" == "install" ]]; then
  rsync -avr --copy-links --no-relative --exclude '*/.svn/*' --files-from="$RESOURCES_TO_COPY" / "${INSTALL_DIR}/${UNLOCALIZED_RESOURCES_FOLDER_PATH}"
fi
rm -f "$RESOURCES_TO_COPY"

if [[ -n "${WRAPPER_EXTENSION}" ]] && [ `xcrun --find actool` ] && [ `find . -name '*.xcassets' | wc -l` -ne 0 ]
then
  case "${TARGETED_DEVICE_FAMILY}" in
    1,2)
      TARGET_DEVICE_ARGS="--target-device ipad --target-device iphone"
      ;;
    1)
      TARGET_DEVICE_ARGS="--target-device iphone"
      ;;
    2)
      TARGET_DEVICE_ARGS="--target-device ipad"
      ;;
    *)
      TARGET_DEVICE_ARGS="--target-device mac"
      ;;
  esac
  find "${PWD}" -name "*.xcassets" -print0 | xargs -0 actool --output-format human-readable-text --notices --warnings --platform "${PLATFORM_NAME}" --minimum-deployment-target "${IPHONEOS_DEPLOYMENT_TARGET}" ${TARGET_DEVICE_ARGS} --compress-pngs --compile "${BUILT_PRODUCTS_DIR}/${UNLOCALIZED_RESOURCES_FOLDER_PATH}"
fi
