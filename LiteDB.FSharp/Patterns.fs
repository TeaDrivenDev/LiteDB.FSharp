﻿namespace LiteDB.FSharp

open Quotations.Patterns
open FSharp.Reflection

module Patterns = 
    open System.Reflection
    
    let rec (|UnionValue|_|) = function 
        | NewUnionCase(info, [ ]) -> 
            FSharpValue.MakeUnion(info, [|  |]) |> Some
        | NewUnionCase(info, [ ProvidedValue(value) ]) -> 
            FSharpValue.MakeUnion(info, [| value |]) |> Some
        | NewUnionCase(info, [ ProvidedValue(arg1);  ProvidedValue(arg2); ]) -> 
            FSharpValue.MakeUnion(info, [| arg1; arg2; |]) |> Some
        | NewUnionCase(info, [ ProvidedValue(arg1);  ProvidedValue(arg2);  ProvidedValue(arg3) ]) -> 
            FSharpValue.MakeUnion(info, [| arg1; arg2; arg3 |]) |> Some
        | NewUnionCase(info, [ ProvidedValue(arg1);  ProvidedValue(arg2);  ProvidedValue(arg3); ProvidedValue(arg4) ]) -> 
            FSharpValue.MakeUnion(info, [| arg1; arg2; arg3; arg4 |]) |> Some
        | NewUnionCase(info, [ ProvidedValue(arg1);  ProvidedValue(arg2);  ProvidedValue(arg3); ProvidedValue(arg4); ProvidedValue(arg5) ]) -> 
            FSharpValue.MakeUnion(info, [| arg1; arg2; arg3; arg4; arg4 |]) |> Some
        | _ -> None

    and (|NewObjectValue|_|) = function 
        | NewObject(ctorInfo, [ ]) -> 
            System.Activator.CreateInstance(ctorInfo.DeclaringType) |> Some 
        | NewObject(ctorInfo, [ ProvidedValue(arg1); ProvidedValue(arg2) ]) ->
            System.Activator.CreateInstance(ctorInfo.DeclaringType, arg1, arg2) |> Some 
        | NewObject(ctorInfo, [ ProvidedValue(arg1); ProvidedValue(arg2); ProvidedValue(arg3) ]) ->
            System.Activator.CreateInstance(ctorInfo.DeclaringType, arg1, arg2, arg3) |> Some 
        | NewObject(ctorInfo, [ ProvidedValue(arg1); ProvidedValue(arg2); ProvidedValue(arg3); ProvidedValue(arg4) ]) ->
            System.Activator.CreateInstance(ctorInfo.DeclaringType, arg1, arg2, arg3, arg4) |> Some 
        | NewObject(ctorInfo, [ ProvidedValue(arg1); ProvidedValue(arg2); ProvidedValue(arg3); ProvidedValue(arg4); ProvidedValue(arg5) ]) ->
            System.Activator.CreateInstance(ctorInfo.DeclaringType, arg1, arg2, arg3, arg4, arg5) |> Some 
        | NewObject(ctorInfo, [ ProvidedValue(arg1); ProvidedValue(arg2); ProvidedValue(arg3); ProvidedValue(arg4); ProvidedValue(arg5); ProvidedValue(arg6) ]) ->
            System.Activator.CreateInstance(ctorInfo.DeclaringType, arg1, arg2, arg3, arg4, arg5, arg6) |> Some 
        | _ -> None 

    and (|RecordValue|_|) = function 
        | NewRecord(recordType, [ ProvidedValue(field) ]) -> 
            FSharpValue.MakeRecord(recordType, [| field |]) |> Some
        | NewRecord(recordType, [ ProvidedValue(arg1); ProvidedValue(arg2); ]) -> 
            FSharpValue.MakeRecord(recordType, [| arg1; arg2; |]) |> Some
        | NewRecord(recordType, [ ProvidedValue(arg1);  ProvidedValue(arg2);  ProvidedValue(arg3); ]) -> 
            FSharpValue.MakeRecord(recordType, [| arg1; arg2; arg3 |]) |> Some
        | NewRecord(recordType, [ ProvidedValue(arg1); ProvidedValue(arg2);  ProvidedValue(arg3); ProvidedValue(arg4) ]) -> 
            FSharpValue.MakeRecord(recordType, [| arg1; arg2; arg3; arg4 |]) |> Some
        | NewRecord(recordType, [ ProvidedValue(arg1);  ProvidedValue(arg2);  ProvidedValue(arg3); ProvidedValue(arg4); ProvidedValue(arg5) ]) -> 
            FSharpValue.MakeRecord(recordType, [| arg1; arg2; arg3; arg4; arg4 |]) |> Some
        | _ -> None

    and (|Tuples|_|) = function 
        | NewTuple [ProvidedValue(arg1); ProvidedValue(arg2)]  ->
            Some (box [arg1; arg2])
        | NewTuple [ProvidedValue(arg1); ProvidedValue(arg2); ProvidedValue(arg3)]  ->
            Some (box [arg1; arg2; arg3]) 
        | NewTuple [ProvidedValue(arg1); ProvidedValue(arg2); ProvidedValue(arg3); ProvidedValue(arg4)]  ->
            Some (box [arg1; arg2; arg3; arg4])
        | NewTuple [ProvidedValue(arg1); ProvidedValue(arg2); ProvidedValue(arg3); ProvidedValue(arg4); ProvidedValue(arg5)]  ->
            Some (box [arg1; arg2; arg3; arg4; arg5])
        | _ -> None
    
    and (| ProvidedValue |_|) = function 
        | Value(value, _ ) -> Some value 
        | ValueWithName(value, _, _) -> Some value
        | UnionValue value -> Some value
        | RecordValue value -> Some value
        | Tuples value -> Some value
        | NewObjectValue value -> Some value 
        | _ -> None

    let (|LogicOp|_|) (info: MethodInfo) = 
        match info.Name with 
        | "op_Equality" -> Some "="
        | "op_NotEqual" -> Some "<>"
        | "op_GreaterThan" -> Some ">" 
        | "op_LessThan" -> Some "<" 
        | "op_GreaterThanOrEqual" -> Some ">=" 
        | "op_LessThanOrEqual" -> Some "<=" 
        | otherwise -> None 

    let (|StringOp|_|) (info: MethodInfo) = 
        match info.DeclaringType.FullName ,info.Name with  
        | "System.String", name -> Some name 
        | _, _ -> None 


    let (|CoreOp|_|) (info: MethodInfo) = 
        match info.DeclaringType.FullName, info.Name with 
        | "Microsoft.FSharp.Core.Operators", "Not" -> Some "not" 
        | _ -> None 
        
    let (|PropertyEqual|_|) = function 
        | Call(_, LogicOp "=", [PropertyGet(_, propInfo, []); ProvidedValue(value)]) -> 
            Some (propInfo.Name, value)
        | otherwise -> 
            None

    let (|PropertyNotEqual|_|) = function 
        | Call(_, LogicOp "<>", [PropertyGet(_, propInfo, []); ProvidedValue(value)]) ->
            Some (propInfo.Name, value) 
        | otherwise -> 
            None
     
    let (|NotProperty|_|) = function 
        | Call(_, CoreOp "not", [expr]) -> 
            Some expr  
        | _ -> None 

    let (|ProperyGreaterThan|_|) = function 
       | Call(_, LogicOp ">", [PropertyGet(_, propInfo, []); ProvidedValue(value)]) ->
           Some (propInfo.Name, value)
       | otherwise -> None

    let (|StringNullOrWhiteSpace|_|) = function 
        | Call(_, StringOp "IsNullOrWhiteSpace", [PropertyGet(_, propInfo, [])]) -> 
            Some (propInfo.Name) 
        | otherwise -> None 

    let (|StringIsNullOrEmpty|_|) = function 
        | Call(_, StringOp "IsNullOrEmpty", [PropertyGet(_, propInfo, [])]) -> 
            Some (propInfo.Name)
        | otherwise -> None 

    let (|StringContains|_|) = function 
        | Call(Some (PropertyGet(_, propInfo, [])), StringOp "Contains",[ProvidedValue(value)]) ->
            Some (propInfo.Name, value) 
        | otherwise -> None 
        
    let (|ProperyGreaterThanOrEqual|_|) = function 
       | Call(_, LogicOp ">=", [PropertyGet(_, propInfo, []); ProvidedValue(value)]) ->
           Some (propInfo.Name, value)
       | otherwise -> None

    let (|PropertyLessThan|_|) = function 
       | Call(_, LogicOp "<", [PropertyGet(_, propInfo, []); ProvidedValue(value)]) ->
           Some (propInfo.Name, value)
       | otherwise -> None

    let (|BooleanGet|_|) = function 
        | PropertyGet(_, propInfo, []) -> 
            Some propInfo.Name 
        | otherwise -> None 
    
    let (|PropertyLessThanOrEqual|_|) = function 
       | Call(_, LogicOp "<=", [PropertyGet(_, propInfo, []); ProvidedValue(value)]) ->
           Some (propInfo.Name, value)
       | otherwise -> None
     
    let (|And|_|) = function 
       | IfThenElse (left, right, Value(value, _)) when unbox<bool> value = false -> 
            Some (left, right) 
       | otherwise -> None

    let (|Or|_|) = function 
       | IfThenElse (left, Value(value, _), right) when unbox<bool> value -> 
            Some (left, right) 
       | otherwise -> None