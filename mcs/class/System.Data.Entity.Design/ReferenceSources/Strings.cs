namespace System.Data.Entity.Design
{
    public static class Strings
    {
		public static string CodeGenSourceFilePathIsNotAFile => SR.GetString (SR.CodeGenSourceFilePathIsNotAFile);
		public static string ConstructorComments => SR.GetString (SR.ConstructorComments);
		public static string EdmSchemaNotValid => SR.GetString (SR.EdmSchemaNotValid);
		public static string EntityCodeGenTargetTooLow => SR.GetString (SR.EntityCodeGenTargetTooLow);
		public static string EntityModelGeneratorSchemaNotLoaded => SR.GetString (SR.EntityModelGeneratorSchemaNotLoaded);
		public static string EntityStoreGeneratorSchemaNotLoaded => SR.GetString (SR.EntityStoreGeneratorSchemaNotLoaded);
		public static string GetViewAtMethodComments => SR.GetString (SR.GetViewAtMethodComments);
		public static string MetadataItemErrorsFoundDuringGeneration => SR.GetString (SR.MetadataItemErrorsFoundDuringGeneration);
		public static string MissingDocumentationNoName => SR.GetString (SR.MissingDocumentationNoName);
		public static string ProviderSchemaErrors => SR.GetString (SR.ProviderSchemaErrors);
		public static string SingleStoreEntityContainerExpected => SR.GetString (SR.SingleStoreEntityContainerExpected);
		public static string TargetEntityFrameworkVersionToNewForEntityClassGenerator => SR.GetString (SR.TargetEntityFrameworkVersionToNewForEntityClassGenerator);
		public static string TypeComments => SR.GetString (SR.TypeComments);
		public static string UnableToGenerateForeignKeyPropertiesForV1 => SR.GetString (SR.UnableToGenerateForeignKeyPropertiesForV1);

        public static string AssociationMissingKeyColumn (object p0, object p1, object p2)
        {
            return SR.GetString (SR.AssociationMissingKeyColumn, p0, p1, p2);
        }

        public static string ColumnFacetValueOutOfRange (object p0, object p1, object p2, object p3, object p4, object p5)
        {
            return SR.GetString (SR.ColumnFacetValueOutOfRange, p0, p1, p2, p3, p4, p5);
        }

        public static string CannotChangePropertyReturnType (object p0, object p1)
        {
            return SR.GetString (SR.CannotChangePropertyReturnType, p0, p1);
        }

        public static string CannotChangePropertyReturnTypeToNull (object p0, object p1)
        {
            return SR.GetString (SR.CannotChangePropertyReturnTypeToNull, p0, p1);
        }

        public static string CannotCreateEntityWithNoPrimaryKeyDefined (object p0)
        {
            return SR.GetString (SR.CannotCreateEntityWithNoPrimaryKeyDefined, p0);
        }

        public static string CtorSummaryComment (object p0)
        {
            return SR.GetString (SR.CtorSummaryComment, p0);
        }

        public static string DbProviderServicesInformationLocationPath (object p0, object p1)
        {
            return SR.GetString (SR.DbProviderServicesInformationLocationPath, p0, p1);
        }

        public static string DefaultTargetVersionTooLow (object p0, object p1)
        {
            return SR.GetString (SR.DefaultTargetVersionTooLow, p0, p1);
        }

        public static string DuplicateClassName (object p0, object p1, object p2)
        {
            return SR.GetString (SR.DuplicateClassName, p0, p1, p2);
        }

        public static string DuplicateEntryInUserDictionary (object p0, object p1)
        {
            return SR.GetString (SR.DuplicateEntryInUserDictionary, p0, p1);
        }

        public static string DuplicateEntityContainerName (object p0, object p1)
        {
            return SR.GetString (SR.DuplicateEntityContainerName, p0, p1);
        }

        public static string EdmSchemaFileNotFound (object p0)
        {
            return SR.GetString (SR.EdmSchemaFileNotFound, p0);
        }

        public static string EmptyCtorSummaryComment (object p0, object p1)
        {
            return SR.GetString (SR.EmptyCtorSummaryComment, p0, p1);
        }

        public static string EntityClient_DoesNotImplementIServiceProvider (object p0)
        {
            return SR.GetString (SR.EntityClient_DoesNotImplementIServiceProvider, p0);
        }

        public static string EntityClient_InvalidStoreProvider (object p0)
        {
            return SR.GetString (SR.EntityClient_InvalidStoreProvider, p0);
        }

        public static string EntityClient_ReturnedNullOnProviderMethod (object p0, object p1)
        {
            return SR.GetString (SR.EntityClient_ReturnedNullOnProviderMethod, p0, p1);
        }

        public static string EntitySetExistsWithDifferentCase (object p0)
        {
            return SR.GetString (SR.EntitySetExistsWithDifferentCase, p0);
        }

        public static string EntityTypeAndSetAccessibilityConflict (object p0, object p1, object p2, object p3)
        {
            return SR.GetString (SR.EntityTypeAndSetAccessibilityConflict, p0, p1, p2, p3);
        }

        public static string ExcludedColumnWasAKeyColumnEntityIsInvalid (object p0, object p1)
        {
            return SR.GetString (SR.ExcludedColumnWasAKeyColumnEntityIsInvalid, p0, p1);
        }

        public static string ExcludedColumnWasAKeyColumnEntityIsReadOnly (object p0, object p1)
        {
            return SR.GetString (SR.ExcludedColumnWasAKeyColumnEntityIsReadOnly, p0, p1);
        }

        public static string FactoryMethodSummaryComment (object p0)
        {
            return SR.GetString (SR.FactoryMethodSummaryComment, p0);
        }

        public static string FactoryParamCommentGeneral (object p0)
        {
            return SR.GetString (SR.FactoryParamCommentGeneral, p0);
        }

        public static string GeneratedPropertyAccessibilityConflict (object p0, object p1, object p2)
        {
            return SR.GetString (SR.GeneratedPropertyAccessibilityConflict, p0, p1, p2);
        }

        public static string GeneratedFactoryMethodNameConflict (object p0, object p1)
        {
            return SR.GetString (SR.GeneratedFactoryMethodNameConflict, p0, p1);
        }

        public static string GeneratedNavigationPropertyNameConflict (object p0, object p1, object p2)
        {
            return SR.GetString (SR.GeneratedNavigationPropertyNameConflict, p0, p1, p2);
        }

        public static string InvalidAttributeSuppliedForType (object p0)
        {
            return SR.GetString (SR.InvalidAttributeSuppliedForType, p0);
        }

        public static string InvalidEntityContainerNameArgument (object p0)
        {
            return SR.GetString (SR.InvalidEntityContainerNameArgument, p0);
        }

        public static string InvalidInterfaceSuppliedForType (object p0)
        {
            return SR.GetString (SR.InvalidInterfaceSuppliedForType, p0);
        }

        public static string InvalidMemberSuppliedForType (object p0)
        {
            return SR.GetString (SR.InvalidMemberSuppliedForType, p0);
        }

        public static string InvalidNamespaceNameArgument (object p0)
        {
            return SR.GetString (SR.InvalidNamespaceNameArgument, p0);
        }

        public static string IndividualViewComments (object p0)
        {
            return SR.GetString (SR.IndividualViewComments, p0);
        }

        public static string InvalidAttributeSuppliedForProperty (object p0)
        {
            return SR.GetString (SR.InvalidAttributeSuppliedForProperty, p0);
        }

        public static string InvalidGetStatementSuppliedForProperty (object p0)
        {
            return SR.GetString (SR.InvalidGetStatementSuppliedForProperty, p0);
        }

        public static string InvalidNonStoreEntityContainer (object p0)
        {
            return SR.GetString (SR.InvalidNonStoreEntityContainer, p0);
        }

        public static string InvalidSetStatementSuppliedForProperty (object p0)
        {
            return SR.GetString (SR.InvalidSetStatementSuppliedForProperty, p0);
		}

        public static string InvalidStringArgument (object p0)
        {
            return SR.GetString (SR.InvalidStringArgument, p0);
		}

        public static string InvalidTypeForPrimaryKey (object p0, object p1, object p2)
        {
            return SR.GetString (SR.InvalidTypeForPrimaryKey, p0, p1, p2);
        }

        public static string ItemExistsWithDifferentCase (object p0, object p1)
        {
            return SR.GetString (SR.ItemExistsWithDifferentCase, p0, p1);
        }

        public static string MissingComplexTypeDocumentation (object p0)
        {
            return SR.GetString (SR.MissingComplexTypeDocumentation, p0);
        }

        public static string MissingDocumentation (object p0)
        {
            return SR.GetString (SR.MissingDocumentation, p0);
        }

        public static string MissingPropertyDocumentation (object p0)
        {
            return SR.GetString (SR.MissingPropertyDocumentation, p0);
        }

        public static string ModelGeneration_UnGeneratableType (object p0)
        {
            return SR.GetString (SR.ModelGeneration_UnGeneratableType, p0);
        }

        public static string NamespaceComments (object p0, object p1)
        {
            return SR.GetString (SR.NamespaceComments, p0, p1);
        }

        public static string NoPrimaryKeyDefined (object p0)
        {
            return SR.GetString (SR.NoPrimaryKeyDefined, p0);
        }

        public static string NullAdditionalSchema (object p0, object p1)
        {
            return SR.GetString (SR.NullAdditionalSchema, p0, p1);
        }

        public static string ParameterDirectionNotValid (object p0, object p1, object p2)
        {
            return SR.GetString (SR.ParameterDirectionNotValid, p0, p1, p2);
        }

        public static string PropertyExistsWithDifferentCase (object p0)
        {
            return SR.GetString (SR.PropertyExistsWithDifferentCase, p0);
        }

        public static string ProviderFactoryReturnedNullFactory (object p0)
        {
            return SR.GetString (SR.ProviderFactoryReturnedNullFactory, p0);
        }

        public static string ReservedNamespace (object p0)
        {
            return SR.GetString (SR.ReservedNamespace, p0);
        }

        public static string Serialization_UnknownGlobalItem (object p0)
        {
            return SR.GetString (SR.Serialization_UnknownGlobalItem, p0);
        }

        public static string SharedForeignKey (object p0, object p1, object p2)
        {
            return SR.GetString (SR.SharedForeignKey, p0, p1, p2);
        }

        public static string StonglyTypedAccessToNullValue (object p0, object p1)
        {
            return SR.GetString (SR.StonglyTypedAccessToNullValue, p0, p1);
        }

        public static string TableReferencedByAssociationWasNotFound (object p0)
        {
            return SR.GetString (SR.TableReferencedByAssociationWasNotFound, p0);
        }

        public static string TableReferencedByTvfWasNotFound (object p0)
        {
            return SR.GetString (SR.TableReferencedByTvfWasNotFound, p0);
        }

        public static string TargetVersionSchemaVersionMismatch (object p0, object p1)
        {
            return SR.GetString (SR.TargetVersionSchemaVersionMismatch, p0, p1);
        }

        public static string UnableToGenerateFunctionImportParameterName (object p0, object p1)
        {
            return SR.GetString (SR.UnableToGenerateFunctionImportParameterName, p0, p1);
        }

        public static string UnmappedFunctionImport (object p0)
        {
            return SR.GetString (SR.UnmappedFunctionImport, p0);
        }

        public static string UnsupportedDbRelationship (object p0)
        {
            return SR.GetString (SR.UnsupportedDbRelationship, p0);
        }

        public static string UnsupportedDataType (object p0, object p1, object p2)
        {
            return SR.GetString (SR.UnsupportedDataType, p0, p1, p2);
        }

        public static string UnsupportedDataTypeForTarget (object p0, object p1, object p2)
        {
            return SR.GetString (SR.UnsupportedDataTypeForTarget, p0, p1, p2);
        }

        public static string UnsupportedDataTypeUnknownType (object p0, object p1)
        {
            return SR.GetString (SR.UnsupportedDataTypeUnknownType, p0, p1);
        }

        public static string UnsupportedForeignKeyPattern (object p0, object p1, object p2, object p3)
        {
            return SR.GetString (SR.UnsupportedForeignKeyPattern, p0, p1, p2, p3);
        }

        public static string UnsupportedFunctionReturnDataType (object p0, object p1)
        {
            return SR.GetString (SR.UnsupportedFunctionReturnDataType, p0, p1);
        }

        public static string UnsupportedFunctionReturnDataTypeForTarget (object p0, object p1)
        {
            return SR.GetString (SR.UnsupportedFunctionReturnDataTypeForTarget, p0, p1);
        }

        public static string UnsupportedFunctionParameterDataType (object p0, object p1, object p2, object p3)
        {
            return SR.GetString (SR.UnsupportedFunctionParameterDataType, p0, p1, p2, p3);
        }

        public static string UnsupportedFunctionParameterDataTypeForTarget (object p0, object p1, object p2, object p3)
        {
            return SR.GetString (SR.UnsupportedFunctionParameterDataTypeForTarget, p0, p1, p2, p3);
        }

        public static string UnsupportedLocaleForPluralizationServices (object p0)
        {
            return SR.GetString (SR.UnsupportedLocaleForPluralizationServices, p0);
        }

        public static string UnsupportedQueryViewInEntityContainerMapping (object p0)
        {
            return SR.GetString (SR.UnsupportedQueryViewInEntityContainerMapping, p0);
        }
    }
}
