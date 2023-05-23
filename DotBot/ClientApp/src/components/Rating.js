import React, { Component } from "react";
import { Table } from "reactstrap";
import { RatingItem } from "./RatingItem";

export class Rating extends Component{

    constructor(props){
        super(props);
        this.state = {data: [], loading: true}
    }

    async FetchData(){
        const response = await fetch('https://a17660-bc6d.k.d-f.pw/rating/data')
        const text = await response.text()
        const json = JSON.parse(text)
        this.setState({data: json, loading: false})
        
    }
    static renderRating(data){
    return(
                     
        <Table>
            <thead>             
            <tr>
                <td>id</td>
                <td>Имя</td>
                <td>Уровень</td>
                <td>Общая мощь</td>
            </tr>
            </thead>
            <tbody>
                {data.map(item => <RatingItem key={item.Id} item={item}/>)}       
            </tbody>
        
        </Table>
            
        
        )
    }

    componentDidMount(){
        this.FetchData()
    }

    render(){
        let contents = this.state.loading
      ? <p><em>Loading...</em></p>
      : Rating.renderRating(this.state.data); 
        return(
            <div>
                {contents}
            </div>
            )
    }
}
