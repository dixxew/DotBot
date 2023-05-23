import React, { Component } from "react";
import { Table } from "reactstrap";

export class Char extends Component{

    constructor(props){
        super(props);
        this.state = {data: [], loading: true}
    }

    async FetchData(){
        const response = await fetch('https://a17660-bc6d.k.d-f.pw/char/data?id=1')
        const text = await response.text()
        const json = JSON.parse(text)
        this.setState({data: json, loading: false})
        
    }
    static renderChar(data){
    return(
        <div>
            <h3>{data.Name}</h3>
            <Table>                
                <tbody>
                    <td>
                        <tr><img src={data.AvatarUrl}></img></tr>
                    </td>
                    <td>
                        <tr><h5>Уровень: {data.Level} | {data.LevelPoints} очков доступно</h5></tr>
                        <tr>{data.Exp} / {data.MaxExp}</tr>
                        <tr></tr>
                        <tr>Kills: {data.Kills}</tr>
                        <tr>Атака: {data.Power} (+{data.WeaponPower})</tr>
                        <tr>Защита: {data.Defence} (+{data.ArmorDefence})</tr>
                        <tr>GOLD: {data.Money} </tr>
                    </td>      
                </tbody>        
            </Table>
        </div>
        )
    }

    componentDidMount(){
        this.FetchData()
    }

    render(){
        let contents = this.state.loading
      ? <p><em>Loading...</em></p>
      : Char.renderChar(this.state.data); 
        return(
            <div>
                {contents}
            </div>
            )
    }
}
