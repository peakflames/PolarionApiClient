# TrackerWebService Interface Methods (Implementation Tracking)

## WorkItem Methods

| Method                                      | Planned | Implemented |
| ------------------------------------------- | ------- | ----------- |
| createWorkItemAsync                         |         |             |
| createWorkItemInModuleAsync                 |         |             |
| deleteWorkItemAsync                         |         |             |
| getWorkItemByIdAsync                        | ✅       | ✅           |
| getWorkItemByIdsWithFieldsAsync             |         |             |
| getWorkItemByUriAsync                       |         |             |
| getWorkItemByUriInRevisionAsync             |         |             |
| getWorkItemByUriInRevisionWithFieldsAsync   |         |             |
| getWorkItemByUriWithFieldsAsync             |         |             |
| getWorkItemsCountAsync                      |         |             |
| getWorkItemsLinkedToRevisionAsync           |         |             |
| getWorkItemsLinkedToRevisionWithFieldsAsync |         |             |
| moveWorkItemToDocumentAsync                 |         |             |
| moveWorkItemToModuleAsync                   |         |             |
| queryWorkItemUrisAsync                      |         |             |
| queryWorkItemUrisBySQLAsync                 |         |             |
| queryWorkItemUrisInBaselineAsync            |         |             |
| queryWorkItemUrisInBaselineBySQLAsync       |         |             |
| queryWorkItemUrisInBaselineLimitedAsync     |         |             |
| queryWorkItemUrisLimitedAsync               |         |             |
| queryWorkItemsAsync                         | ✅       | ✅           |
| queryWorkItemsBySQLAsync                    |         |             |
| queryWorkItemsInBaselineAsync               |         |             |
| queryWorkItemsInBaselineBySQLAsync          |         |             |
| queryWorkItemsInBaselineLimitedAsync        |         |             |
| queryWorkItemsInRevisionAsync               | ✅       | ✅           |
| queryWorkItemsInRevisionLimitedAsync        |         |             |
| queryWorkItemsLimitedAsync                  |         |             |
| updateWorkItemAsync                         |         |             |
| getLocalizedWorkItemDescriptionAsync        |         |             |
| getLocalizedWorkItemDescriptionFieldAsync   |         |             |
| getLocalizedWorkItemTitleAsync              |         |             |
| getLocalizedWorkItemTitleFieldAsync         |         |             |
| setLocalizedWorkItemDescriptionAsync        |         |             |
| setLocalizedWorkItemTitleAsync              |         |             |
| getBackLinkedWorkitemsAsync                 |         |             |

## Document Methods

| Method                                | Planned | Implemented |
| ------------------------------------- | ------- | ----------- |
| createDocumentAsync                   |         |             |
| createDocumentCommentAsync            |         |             |
| createDocumentCommentReferringWIAsync |         |             |
| createDocumentCommentReplyAsync       |         |             |
| exportDocumentToPDFAsync              |         |             |
| getDocumentLocationsAsync             |         |             |
| getDocumentSpacesAsync                |         |             |
| reuseDocumentAsync                    |         |             |
| updateDerivedDocumentAsync            |         |             |
| updateTitleHeadingInDocumentAsync     |         |             |

## Module Methods

| Method                              | Planned | Implemented |
| ----------------------------------- | ------- | ----------- |
| createModuleAsync                   |         |             |
| deleteModuleAsync                   |         |             |
| getModuleByLocationAsync            |         |             |
| getModuleByLocationWithFieldsAsync  |         |             |
| getModuleByUriAsync                 |         |             |
| getModuleByUriWithFieldsAsync       |         |             |
| getModuleUrisAsync                  |         |             |
| getModuleWorkItemUrisAsync          |         |             |
| getModuleWorkItemsAsync             |         |             |
| getModulesAsync                     |         |             |
| getModulesSubFoldersAsync           |         |             |
| getModulesWithFieldsAsync           |         |             |
| queryModuleUrisAsync                |         |             |
| queryModuleUrisBySQLAsync           |         |             |
| queryModuleUrisInBaselineAsync      |         |             |
| queryModuleUrisInBaselineBySQLAsync |         |             |
| queryModulesAsync                   |         |             |
| queryModulesBySQLAsync              |         |             |
| queryModulesInBaselineAsync         |         |             |
| queryModulesInBaselineBySQLAsync    |         |             |
| reuseModuleAsync                    |         |             |
| updateDerivedModuleAsync            |         |             |
| updateModuleAsync                   |         |             |

## Comment Methods

| Method                                  | Planned | Implemented |
| --------------------------------------- | ------- | ----------- |
| addCommentAsync                         |         |             |
| addCommentToActivityAsync               |         |             |
| canCommentActivityAsync                 |         |             |
| canCurrentUserAddCommentToActivityAsync |         |             |
| createCommentAsync                      |         |             |
| createCommentNewAsync                   |         |             |
| createDocumentCommentAsync              |         |             |
| createDocumentCommentReferringWIAsync   |         |             |
| createDocumentCommentReplyAsync         |         |             |
| isResolvedCommentAsync                  |         |             |
| setCommentTagsAsync                     |         |             |
| setResolvedCommentAsync                 |         |             |

## Attachment Methods

| Method                | Planned | Implemented |
| --------------------- | ------- | ----------- |
| createAttachmentAsync |         |             |
| deleteAttachmentAsync |         |             |
| getAttachmentAsync    |         |             |
| updateAttachmentAsync |         |             |

## Approval Methods

| Method                   | Planned | Implemented |
| ------------------------ | ------- | ----------- |
| addApproveeAsync         |         |             |
| editApprovalAsync        |         |             |
| getAllowedApproversAsync |         |             |
| removeApproveeAsync      |         |             |

## Assignment Methods

| Method                   | Planned | Implemented |
| ------------------------ | ------- | ----------- |
| addAssigneeAsync         |         |             |
| doAutoassignAsync        |         |             |
| getAllowedAssigneesAsync |         |             |
| removeAssigneeAsync      |         |             |

## Category Methods

| Method              | Planned | Implemented |
| ------------------- | ------- | ----------- |
| addCategoryAsync    |         |             |
| getCategoriesAsync  |         |             |
| removeCategoryAsync |         |             |

## Enum Methods

| Method                                | Planned | Implemented |
| ------------------------------------- | ------- | ----------- |
| getAllEnumOptionIdsForIdAsync         |         |             |
| getAllEnumOptionIdsForKeyAsync        |         |             |
| getAllEnumOptionsForIdAsync           |         |             |
| getAllEnumOptionsForKeyAsync          |         |             |
| getAvailableEnumOptionIdsForIdAsync   |         |             |
| getAvailableEnumOptionIdsForKeyAsync  |         |             |
| getEnumControlKeyForIdAsync           |         |             |
| getEnumControlKeyForKeyAsync          |         |             |
| getEnumOptionFromObjectUriForIdAsync  |         |             |
| getEnumOptionFromObjectUriForKeyAsync |         |             |
| getEnumOptionWithEnumIdAsync          |         |             |
| getEnumOptionWithKeyAsync             |         |             |
| getEnumOptionsForIdAsync              |         |             |
| getEnumOptionsForIdWithControlAsync   |         |             |
| getEnumOptionsForKeyAsync             |         |             |
| getEnumOptionsForKeyWithControlAsync  |         |             |
| getFilteredEnumOptionsForKeyAsync     |         |             |

## Wiki Methods

| Method                                | Planned | Implemented |
| ------------------------------------- | ------- | ----------- |
| getWikiPageByUriAsync                 |         |             |
| getWikiPageByUriWithFieldsAsync       |         |             |
| getWikiPageUrisAsync                  |         |             |
| getWikiPagesAsync                     |         |             |
| getWikiPagesWithFieldsAsync           |         |             |
| getWikiSpacesAsync                    |         |             |
| queryWikiPageUrisAsync                |         |             |
| queryWikiPageUrisBySQLAsync           |         |             |
| queryWikiPageUrisInBaselineAsync      |         |             |
| queryWikiPageUrisInBaselineBySQLAsync |         |             |
| queryWikiPagesAsync                   |         |             |
| queryWikiPagesBySQLAsync              |         |             |
| queryWikiPagesInBaselineAsync         |         |             |
| queryWikiPagesInBaselineBySQLAsync    |         |             |

## Revision Methods

| Method                                      | Planned | Implemented |
| ------------------------------------------- | ------- | ----------- |
| addExternalLinkedRevisionAsync              |         |             |
| addLinkedRevisionAsync                      |         |             |
| getRevisionAsync                            |         |             |
| getRevisionByUriAsync                       |         |             |
| getRevisionsAsync                           |         |             |
| getWorkItemByUriInRevisionAsync             |         |             |
| getWorkItemByUriInRevisionWithFieldsAsync   |         |             |
| getWorkItemsLinkedToRevisionAsync           |         |             |
| getWorkItemsLinkedToRevisionWithFieldsAsync |         |             |
| queryRevisionUrisAsync                      |         |             |
| queryRevisionsAsync                         |         |             |
| queryWorkItemsInRevisionAsync               | ✅       | ✅           |
| queryWorkItemsInRevisionLimitedAsync        |         |             |
| removeExternalLinkedRevisionAsync           |         |             |
| removeLinkedRevisionAsync                   |         |             |

## Activity Methods

| Method                                  | Planned | Implemented |
| --------------------------------------- | ------- | ----------- |
| addCommentToActivityAsync               |         |             |
| canCommentActivityAsync                 |         |             |
| canCurrentUserAddCommentToActivityAsync |         |             |
| getActivityByGlobalIdAsync              |         |             |
| listActivitySourcesAsync                |         |             |
| listAllActivitiesAsync                  |         |             |
| listAllActivitiesGlobalIdsAsync         |         |             |
| listGroupActivitiesAsync                |         |             |
| listGroupActivitiesGlobalIdsAsync       |         |             |
| listProjectActivitiesAsync              |         |             |
| listProjectActivitiesGlobalIdsAsync     |         |             |

## Folder Methods

| Method                    | Planned | Implemented |
| ------------------------- | ------- | ----------- |
| createFolderAsync         |         |             |
| getChildFoldersAsync      |         |             |
| getFolderAsync            |         |             |
| getFolderForURIAsync      |         |             |
| getFoldersAsync           |         |             |
| getModulesSubFoldersAsync |         |             |
| getRootFoldersAsync       |         |             |

## Baseline Methods

| Method                                  | Planned | Implemented |
| --------------------------------------- | ------- | ----------- |
| createBaselineAsync                     |         |             |
| deleteBaselineAsync                     |         |             |
| queryBaselinesAsync                     |         |             |
| queryModuleUrisInBaselineAsync          |         |             |
| queryModuleUrisInBaselineBySQLAsync     |         |             |
| queryModulesInBaselineAsync             |         |             |
| queryModulesInBaselineBySQLAsync        |         |             |
| queryWikiPageUrisInBaselineAsync        |         |             |
| queryWikiPageUrisInBaselineBySQLAsync   |         |             |
| queryWikiPagesInBaselineAsync           |         |             |
| queryWikiPagesInBaselineBySQLAsync      |         |             |
| queryWorkItemUrisInBaselineAsync        |         |             |
| queryWorkItemUrisInBaselineBySQLAsync   |         |             |
| queryWorkItemUrisInBaselineLimitedAsync |         |             |
| queryWorkItemsInBaselineAsync           |         |             |
| queryWorkItemsInBaselineBySQLAsync      |         |             |
| queryWorkItemsInBaselineLimitedAsync    |         |             |
| updateBaselineAsync                     |         |             |

## Workflow Methods

| Method                                         | Planned | Implemented |
| ---------------------------------------------- | ------- | ----------- |
| getAvailableActionsAsync                       |         |             |
| getInitialWorkflowActionAsync                  |         |             |
| getInitialWorkflowActionForProjectAndTypeAsync |         |             |
| getUnavailableActionsAsync                     |         |             |
| performWorkflowActionAsync                     |         |             |
| resetWorkflowAsync                             |         |             |

## Custom Field Methods

| Method                          | Planned | Implemented |
| ------------------------------- | ------- | ----------- |
| getCustomFieldAsync             |         |             |
| getCustomFieldKeysAsync         |         |             |
| getCustomFieldTypeAsync         |         |             |
| getCustomFieldTypesAsync        |         |             |
| getDefinedCustomFieldKeysAsync  |         |             |
| getDefinedCustomFieldTypeAsync  |         |             |
| getDefinedCustomFieldTypesAsync |         |             |
| setCustomFieldAsync             |         |             |
| setFieldsNullAsync              |         |             |

## Linking Methods

| Method                          | Planned | Implemented |
| ------------------------------- | ------- | ----------- |
| addExternallyLinkedItemAsync    |         |             |
| addHyperlinkAsync               |         |             |
| addLinkedItemAsync              |         |             |
| addLinkedItemWithRevAsync       |         |             |
| addLinkedOslcItemAsync          |         |             |
| getBackLinkedWorkitemsAsync     |         |             |
| getLinkedResourcesAsync         |         |             |
| removeExternallyLinkedItemAsync |         |             |
| removeHyperlinkAsync            |         |             |
| removeLinkedItemAsync           |         |             |

## Work Record Methods

| Method                                  | Planned | Implemented |
| --------------------------------------- | ------- | ----------- |
| createWorkRecordAsync                   |         |             |
| createWorkRecordWithTypeAndCommentAsync |         |             |
| deleteWorkRecordAsync                   |         |             |

## Planning Methods

| Method                       | Planned | Implemented |
| ---------------------------- | ------- | ----------- |
| addPlaningContraintAsync     |         |             |
| getDurationHoursAsync        |         |             |
| getOneDayLengthAsync         |         |             |
| removePlaningConstraintAsync |         |             |

## Language Methods

| Method                            | Planned | Implemented |
| --------------------------------- | ------- | ----------- |
| getDefaultLanguageDefinitionAsync |         |             |
| getLanguageDefinitionAsync        |         |             |
| getLanguageDefinitionsAsync       |         |             |

## History Methods

| Method                  | Planned | Implemented |
| ----------------------- | ------- | ----------- |
| generateHistoryAsync    |         |             |
| isHistoryAvailableAsync |         |             |

## Time Methods

| Method             | Planned | Implemented |
| ------------------ | ------- | ----------- |
| getTimepointsAsync |         |             |

## Suspect Methods

| Method             | Planned | Implemented |
| ------------------ | ------- | ----------- |
| doAutoSuspectAsync |         |             |
